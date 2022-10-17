using System;
using System.Threading.Tasks;
using FileUpload.Configuration;
using Microsoft.Extensions.Configuration;

namespace FileUpload;

/// <summary>
/// This program reads and validates required configuration before running the application.
/// </summary>
public class Program
{
    static async Task Main()
    {
        var endpointSettings = ReadConfiguration();

        var httpClientFactory = new ServiceHttpClientFactory(endpointSettings.Client);

        var app = new App(httpClientFactory, endpointSettings.FileService);
        await app.Run();
    }

    /// <summary>
    /// User secrets can be set using dotnet core cli for a console application.
    /// </summary>
    /// <example>
    /// dotnet user-secrets set "serviceEndpoints:certificateCommonName" "the common name part of the provided certificate"
    /// etc...
    /// </example>
    /// <returns></returns>
    private static ServiceEndpointsSection ReadConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile("appSettings.json")
            .AddUserSecrets<Program>();

        var settings = builder.Build().GetSection("serviceEndpoints").Get<ServiceEndpointsSection>();

        if (!settings.IsValid)
        {
            throw new Exception("One or more endpoint settings are not correct");
        }
        return settings;
    }
}
