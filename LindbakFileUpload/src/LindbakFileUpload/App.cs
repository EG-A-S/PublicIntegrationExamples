using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LindbakFileUpload.Models;

namespace LindbakFileUpload
{
    /// <summary>
    /// This application generate some dummy item group data and upload that as a file to Lindbak file service.
    /// </summary>
    public class App
    {
        private readonly LindbakHttpClientFactory _httpClientFactory;
        private readonly LindbakEndpoint _fileServiceEndpoint;

        public App(LindbakHttpClientFactory httpClientFactory, LindbakEndpoint fileServiceEndpoint)
        {
            _httpClientFactory = httpClientFactory;
            _fileServiceEndpoint = fileServiceEndpoint;
        }

        public async Task Run()
        {
            var httpClient = await _httpClientFactory.Create(_fileServiceEndpoint);

            const string fileName = "test.jsonl.gz";
            var initializeResponse = await InitializeFileUpload(httpClient, fileName);

            await httpClient.UploadContentAsCompressedJsonLines(
                initializeResponse.ContentUrl, 
                fileName, 
                GenerateItemGroupTestData(100));
        }

        private static async Task<FileUploadInitializeResponse> InitializeFileUpload(HttpClient httpClient, string fileName)
        {
            var initializeRequest = new FileUploadInitializeRequest
            {
                FileName = fileName,
                FileType = "ItemGroup",
                ContentType = "application/x-jsonlines",
                ContentEncoding = "gzip",
                Source = new FileSource
                {
                    Name = "LindbakFileUpload",
                    Reference = Guid.NewGuid().ToString(),
                    TimestampUtc = DateTimeOffset.UtcNow,
                    UserIdentity = "LindbakFileUpload"
                }
            };

            return await httpClient.PostAsJsonAsync<FileUploadInitializeRequest, FileUploadInitializeResponse>(
                    "fileupload/initialize",
                    initializeRequest);
        }

        private static IEnumerable<ItemGroup> GenerateItemGroupTestData(int count)
        {
            return Enumerable
                .Range(1, count)
                .Select(i => 
                new ItemGroup
                {
                    Number = $"{i:0000}",
                    Name = $"ItemGroup {i:0000}",
                    CostCarrier = "LindbakFileUpload"
                });
        }
    }
}