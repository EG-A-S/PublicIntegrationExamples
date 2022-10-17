using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FileUpload.Configuration;
using FileUpload.Models;

namespace FileUpload;

/// <summary>
/// This application generates some dummy data and upload that as a file to File service.
/// </summary>
public class App
{
    private readonly ServiceHttpClientFactory _httpClientFactory;
    private readonly FileServiceEndpoint _fileServiceEndpoint;

    public App(ServiceHttpClientFactory httpClientFactory, FileServiceEndpoint fileServiceEndpoint)
    {
        _httpClientFactory = httpClientFactory;
        _fileServiceEndpoint = fileServiceEndpoint;
    }

    public async Task Run()
    {
        Console.WriteLine("Starting uploaded to FileService...");
        var httpClient = await _httpClientFactory.Create(_fileServiceEndpoint);

        const string fileName = "test.jsonl.gz";
        var initializeResponse = await InitializeFileUpload(httpClient, fileName);

        await httpClient.UploadContentAsCompressedJsonLines(
            initializeResponse.ContentUrl, 
            fileName, 
            GenerateFakeFileContents(100));
        Console.WriteLine("File successfully uploaded to FileService");
    }

    private static async Task<FileUploadInitializeResponse> InitializeFileUpload(HttpClient httpClient, string fileName)
    {
        var initializeRequest = new FileUploadInitializeRequest
        {
            FileName = fileName,
            FileType = "MyBlobTypeInFileUploadExample",
            ContentType = "application/x-jsonlines",
            ContentEncoding = "gzip",
            Source = new FileSource
            {
                Name = "FileUpload test client",
                Reference = Guid.NewGuid().ToString(),
                TimestampUtc = DateTimeOffset.UtcNow,
                UserIdentity = "FileUpload"
            }, 
            Version = "1",
            FileProperties = JsonDocument.Parse(@"{ ""MyCustomProperty"": ""MyValue""  }")
        };

        return await httpClient.PostAsJsonAsync<FileUploadInitializeRequest, FileUploadInitializeResponse>(
                "fileupload/initialize",
                initializeRequest);
    }

    private static IEnumerable<ItemGroup> GenerateFakeFileContents(int count)
    {
        return Enumerable
            .Range(1, count)
            .Select(i => 
            new ItemGroup
            {
                Number = $"{i:0000}",
                Name = $"ItemGroup {i:0000}",
                CostCarrier = "FileUpload"
            });
    }
}
