using System;
using System.IO;
using System.Threading.Tasks;
using ExportSubscriber.Models;
using Azure.Storage.Blobs;
using System.Threading;

namespace ExportSubscriber.Azure.BlobStorage
{
    /// <summary>
    /// Downloads blobs from Azure Storage using a temporary connectionstring (SAS Token)
    /// </summary>
    public class BlobDownloader
    {
        private readonly Uri[] _blobBaseUrlsWithToken;

        public BlobDownloader(Uri[] blobBaseUrlsWithToken)
        {
            _blobBaseUrlsWithToken = blobBaseUrlsWithToken;
        }

        public async Task DownloadBlobAsync(AvailableBlobEvent availableBlobEvent, FileSystemInfo blobLocalStorage, CancellationToken cancellationToken)
        {
            var sasUri = new UriBuilder(availableBlobEvent.Uri)
            {
                Query = FindToken(availableBlobEvent.Uri)
            };

            var client = new BlobClient(sasUri.Uri);

            await client.DownloadToAsync(blobLocalStorage.FullName, cancellationToken);
        }

        private string FindToken(Uri blobUri)
        {
            foreach (var baseUrl in _blobBaseUrlsWithToken)
            {
                var b = baseUrl.GetLeftPart(UriPartial.Path);
                if (blobUri.AbsoluteUri.StartsWith(b, StringComparison.InvariantCultureIgnoreCase))
                {
                    return baseUrl.Query;
                }
            }
            throw new Exception($"Unable to find a token for the url '{blobUri}'");
        }
    }
}
