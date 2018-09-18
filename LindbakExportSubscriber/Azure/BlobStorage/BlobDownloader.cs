using System;
using System.IO;
using System.Threading.Tasks;
using LindbakExportSubscriber.Models;
using Microsoft.WindowsAzure.Storage.Blob;

namespace LindbakExportSubscriber.Azure.BlobStorage
{
    public class BlobDownloader
    {
        private readonly string _sasToken;

        public BlobDownloader(string sasToken)
        {
            _sasToken = sasToken;
        }

        public async Task DownloadBlobAsync(AvailableBlobEvent availableBlobEvent, FileSystemInfo blobLocalStorage)
        {
            var uriBuilder = new UriBuilder(new Uri(availableBlobEvent.Uri))
            {
                Query = _sasToken
            };
            var cloudBlockBlob = new CloudBlockBlob(uriBuilder.Uri);
            await cloudBlockBlob.DownloadToFileAsync(blobLocalStorage.FullName, FileMode.Create);
        }
    }
}