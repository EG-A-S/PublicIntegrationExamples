using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace FileUpload
{
    /// <summary>
    /// This program reads and validates required configuration before running the application.
    /// </summary>
    public class Program
    {
        static async Task Main()
        {
            var configuration = ReadConfiguration();
            var endpointSettings = configuration.GetSection("serviceEndpoints").Get<ServiceEndpointsSection>();

            if (!endpointSettings.IsValid)
            {
                throw new Exception("One or more endpoint settings are not correct");
            }
            
            var httpClientFactory = new ServiceHttpClientFactory(endpointSettings.CertificateCommonName);

            var app = new App(httpClientFactory, endpointSettings.FileService);
            await app.Run();
        }

        /// <summary>
        /// User secrets can be set using dotnet core cli for a console application.
        /// </summary>
        /// <example>
        /// dotnet user-secrets set "serviceEndpoints:certificateCommonName" "the common name part of the provided certificate"
        /// dotnet user-secrets set "serviceEndpoints:fileService:applicationId" "GUID"
        /// </example>
        /// <returns></returns>
        private static IConfigurationRoot ReadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json")
                .AddUserSecrets<Program>();

            return builder.Build();
        }
    }
}
