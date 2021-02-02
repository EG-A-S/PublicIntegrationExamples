using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using ExportSubscriber.Azure.BlobStorage;
using ExportSubscriber.Azure.ServiceBus;
using ExportSubscriber.LocalStorage;
using ExportSubscriber.Models;
using ExportSubscriber.Security;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace ExportSubscriber
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
            _tenantService = new TenantService(_config);

            await RefreshSecrets();
            await ReCreateServiceBusClient();
            ReCreateBlobClient();
            Console.WriteLine("Listening for new messages...");
        }

        private async Task ReCreateServiceBusClient()
        {
            if (_subscriptionClient != null)
            {
                await _subscriptionClient.StopListening();
            }

            _subscriptionClient = ExportSubscriptionClient.Create(_currentSecrets.serviceBusSubscriptionConnectionString, _currentSecrets.serviceBusSubscriptionName);
            _subscriptionClient.StartListening(MessageHandler, MessageHandler_ExceptionHandler);
        }

        private void ReCreateBlobClient()
        {
            _blobDownloader = new BlobDownloader(_currentSecrets.blobBaseUrlsWithToken);
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

            // Note: About exception handling
            // Any exception leaving this method will cause the message to be retried later.
            // If you consider the exception to be permanent, complete the message.
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

            Console.WriteLine(
                $"Got message of type '{metadata.BlobType}' and with correlation-id '{metadata.CorrelationId}', uri '{metadata.Uri}'. Content had format '{metadata.ContentType}'");

            // Save incoming blob to a temporary file. This could of course be done in memory, or to some database
            var blobLocalStorage = LocalStorageUtility.GetNewWorkingFile();
            await _blobDownloader.DownloadBlobAsync(metadata, blobLocalStorage, cancellationToken);

            await ProcessFile(message.MessageId, metadata, blobLocalStorage);
            await _subscriptionClient.CompleteMessage(message);

            LocalStorageUtility.RemoveWorkingFile(blobLocalStorage);
        }

        private async Task MessageHandler_ExceptionHandler(ExceptionReceivedEventArgs args)
        {
            // Check if connection info to ServiceBus has expired, if so, renew
            if (args.Exception is UnauthorizedException)
            {
                await RefreshSecrets();
                await ReCreateServiceBusClient();
                return;
            }

            // Check if connection info to BlobStorage has expired, if so, renew
            if (args.Exception is RequestFailedException blobDownloadException && new[] { 401, 403 }.Contains(blobDownloadException.Status))
            {
                await RefreshSecrets();
                ReCreateBlobClient();
                return;
            }

            // TODO: Add general error handling
            Console.WriteLine(args.Exception.Message);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Process the blob that has arrived.
        /// Ideally this would just dump the file to some persistent queue, and have a separate job process it.
        /// By doing that, you would quickly receieve all available data and then safely and efficiently process it.
        /// In this example, we just read each line, print some info about it and dump it to a file (in the same folder for each blob)
        /// </summary>
        private async Task ProcessFile(string uniqueMessageId, AvailableBlobEvent metadata, FileInfo blobLocalStorage)
        {
            if (!Equal("testdata", metadata.BlobType)) // This is the only supported BlobType by this job.
                return;

            using var fileStream = blobLocalStorage.OpenRead();
            using var uncompressedStream = UncompressIfNeeded(metadata, fileStream);
            using var streamReader = new StreamReader(uncompressedStream, Encoding.UTF8);

            ThrowIfNotJsonLines(metadata);

            var batchOutputFolder = LocalStorageUtility.CreateMessageOutputFolder(_config.OutputDirectory, uniqueMessageId);

            string jsonLine;
            var i = 0;
            while ((jsonLine = await streamReader.ReadLineAsync()) != null)
            {
                // Deserialize the line according to the contract and do something to it (in this example, just print it and dump to a file)
                var testData = JsonConvert.DeserializeObject<TestData>(jsonLine);
                Console.WriteLine($" - Imported id={testData.Id}, name={testData.Name}, # of subitems {testData.Subitems?.Length}");

                await LocalStorageUtility.WriteToFile(batchOutputFolder, i++, jsonLine);
            }
            await LocalStorageUtility.WriteMetadataToFile(batchOutputFolder, metadata);
        }

        private void ThrowIfNotJsonLines(AvailableBlobEvent metadata)
        {
            if (!Equal("application/x-jsonlines", metadata.ContentType))
                throw new ArgumentException($"Blob content is {metadata.ContentType} which is not supported in this job.");
        }

        private static Stream UncompressIfNeeded(AvailableBlobEvent metadata, Stream fileStream)
        {
            return Equals("gzip", metadata.ContentEncoding) ? new GZipStream(fileStream, CompressionMode.Decompress) : fileStream;
        }

        private static bool Equal(string str1, string str2)
        {
            return string.Equals(str1, str2, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
