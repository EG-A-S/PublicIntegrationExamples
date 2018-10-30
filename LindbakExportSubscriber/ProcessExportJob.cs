using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LindbakExportSubscriber.Azure.BlobStorage;
using LindbakExportSubscriber.Azure.ServiceBus;
using LindbakExportSubscriber.LocalStorage;
using LindbakExportSubscriber.Models;
using LindbakExportSubscriber.Security;
using Microsoft.Azure.ServiceBus;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;

namespace LindbakExportSubscriber
{
    public class ProcessExportJob
    {
        private ExportSubscriptionClient _subscriptionClient;
        private BlobDownloader _blobDownloader;
        private TenantService _tenantService;
        private ConnectionSecrets _currentSecrets;
        private readonly Config _config;

        public ProcessExportJob(Config config)
        {
            _config = config;
        }

        public async Task Start()
        {
            _tenantService = new TenantService(new Uri(_config.TenantServiceUrl), _config.TenantServiceCertificateName, _config.TenantServiceResourceId );

            await RefreshSecrets();

            await ReCreateServiceBusClient();

            ReCreateBlobClient();
        }

        private async Task ReCreateServiceBusClient()
        {
            if (_subscriptionClient != null)
            {
                await _subscriptionClient.StopListening();
            }

            _subscriptionClient = ExportSubscriptionClient.Create(_currentSecrets.ServiceBusConnectionstring, _currentSecrets.ServiceBusSubscriptionName );
            _subscriptionClient.StartListening(MessageHandler, MessageHandler_ExceptionHandler);
        }

        private void ReCreateBlobClient()
        {
            _blobDownloader = new BlobDownloader(_currentSecrets.BlobSasToken);
        }

        private async Task RefreshSecrets()
        {
            var shouldRefresh = _currentSecrets == null ||
                                _currentSecrets.TimeUpdatedUtc.AddMinutes(5) < DateTimeOffset.UtcNow;

            if (!shouldRefresh)
            {
                Console.WriteLine("Can not refresh secrets now, already tried recently.");
                _subscriptionClient.StartPauseForMinutes(5);
                return;
            }

            Console.WriteLine("Getting new secrets...");
            _currentSecrets = await _tenantService.GetSecrets();
        }

        private async Task MessageHandler(Message message, CancellationToken cancellationToken)
        {
            if (await _subscriptionClient.WaitForPause(message, cancellationToken))
            {
                return;
            }

            // Note: If anything in this method throws an exception, the message-handling will be retried later.
            // To prevent this, catch the exception here, figure out if it is transient or permanent and complete the message depending on that.
            // This is just examples of error handling
            AvailableBlobEvent metadata;
            try
            {
                metadata = message.DeserializeJsonBody<AvailableBlobEvent>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Permanent error: {ex.Message}");
                await _subscriptionClient.CompleteMessage(message);
                return;
            }

            LogMessage(metadata);

            var blobLocalStorage = LocalStorageUtility.GetNewWorkingFile();

            await _blobDownloader.DownloadBlobAsync(metadata, blobLocalStorage);

            await ProcessFile(message.MessageId, metadata, blobLocalStorage);

            await _subscriptionClient.CompleteMessage(message);

            LocalStorageUtility.RemoveWorkingFile(blobLocalStorage);
        }

        private async Task MessageHandler_ExceptionHandler(ExceptionReceivedEventArgs args)
        {
            if (args.Exception is UnauthorizedException) // Servicebus client authentication error
            {
                await RefreshSecrets();
                await ReCreateServiceBusClient();
                return;
            }

            if (args.Exception is StorageException storageException && new[] { 401, 403 }.Contains(storageException.RequestInformation.HttpStatusCode))
            {
                await RefreshSecrets();
                ReCreateBlobClient();
                return;
            }

            // TODO: Add general error handling
            Console.WriteLine(args.Exception.Message);
            await Task.CompletedTask;
        }

        private async Task ProcessFile(string uniqueMessageId, AvailableBlobEvent metadata, FileInfo blobLocalStorage)
        {
            // Note: Preferably this is done in a separate process, after persisting the message and blob to an internal storage.

            if (!Equal("testdata", metadata.BlobType)) // This is the only supported BlobType by this job.
                return;

            using (var fileStream = blobLocalStorage.OpenRead())
            {
                using (var uncompressedStream = UncompressIfNeeded(metadata, fileStream))
                {
                    using (var streamReader = new StreamReader(uncompressedStream, Encoding.UTF8))
                    {
                        ThrowIfNotJsonLines(metadata);

                        var batchOutputFolder = LocalStorageUtility.CreateMessageOutputFolder(_config.OutputDirectory, uniqueMessageId);

                        string jsonLine;
                        var i = 0;
                        while ((jsonLine = await streamReader.ReadLineAsync()) != null)
                        {
                            var testData = JsonConvert.DeserializeObject<TestData>(jsonLine);
                            Console.WriteLine(
                                $" - Imported id={testData.Id}, name={testData.Name}, # of subitems {testData.Subitems?.Length}");

                            await LocalStorageUtility.WriteToFile(batchOutputFolder, i++, jsonLine);
                        }

                        await LocalStorageUtility.WriteMetadataToFile(batchOutputFolder, metadata);
                    }
                }
            }
        }

        private static void LogMessage(AvailableBlobEvent metadata)
        {
            Console.WriteLine(
                $"Got message of type {metadata.BlobType} and with correlation-id {metadata.CorrelationId}, uri {metadata.Uri}. Content had format {metadata.ContentType}");
        }

        private void ThrowIfNotJsonLines(AvailableBlobEvent metadata)
        {
            if (!Equal("application/x-jsonlines", metadata.ContentType))
                throw new ArgumentException($"Blob content is {metadata.ContentType} which is not supported in this job.");
        }

        private static Stream UncompressIfNeeded(AvailableBlobEvent metadata, Stream fileStream)
        {
            return metadata.ContentEncoding?.ToLower() == "gzip" ? new GZipStream(fileStream, CompressionMode.Decompress) : fileStream;
        }

        private static bool Equal(string str1, string str2)
        {
            return str1.Equals(str2, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}