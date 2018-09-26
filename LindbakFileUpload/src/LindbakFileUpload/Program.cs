using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace LindbakFileUpload
{
    /// <summary>
    /// This program reads and validates required configuration before running the application.
    /// </summary>
    public class Program
    {
        static async Task Main()
        {
            var configuration = ReadConfiguration();
            var endpointSettings = configuration.GetSection("lindbakEndpoints").Get<LindbakEndpointsSection>();

            if (!endpointSettings.IsValid)
            {
                throw new Exception("One or more endpoint settings are not correct");
            }
            
            var httpClientFactory = new LindbakHttpClientFactory(endpointSettings.CertificateCommonName);

            var app = new App(httpClientFactory, endpointSettings.FileService);
            await app.Run();
        }

        /// <summary>
        /// User secrets can be set using dotnet core cli for a console application.
        /// </summary>
        /// <example>
        /// dotnet user-secrets set "lindbakEndpoints:certificateCommonName" "the common name part of the Lindbak provided certificate"
        /// dotnet user-secrets set "lindbakEndpoints:fileService:applicationId" "GUID"
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
