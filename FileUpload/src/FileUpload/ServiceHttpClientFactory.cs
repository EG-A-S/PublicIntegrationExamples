using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using FileUpload.Configuration;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FileUpload;

/// <summary>
/// Factory for creating http clients that are granted access to the APIs.
/// </summary>
/// <remarks>
/// This implementation keeps access tokens in memory and will acquire fresh tokens as needed when
/// new clients are created. Keep in mind that the expiration of those tokens will vary, and the returned
/// http client will not automatically renew the bearer token.
/// </remarks>
public class ServiceHttpClientFactory
{
    private readonly IConfidentialClientApplication _confidentalClient;

    /// <summary>
    /// Create a JSON serializer which follows these conventions:
    /// - property names are in camel case
    /// - enumerations are serialized as strings
    /// </summary>
    /// <returns>JSON serializer using EG Retail conventions</returns>
    public static JsonSerializer CreateCloudBlobSerializer()
    {
        var jsonSerializerSettings = new JsonSerializerSettings
            { ContractResolver = new CamelCasePropertyNamesContractResolver() };
        jsonSerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        return JsonSerializer.Create(jsonSerializerSettings);
    }

    /// <summary>
    /// Initialized a new instance based on the provided certificate.
    /// </summary>
    /// <param name="certificateCommonName">The common name part of the subject of the service certificate</param>
    public ServiceHttpClientFactory(ClientAuthentication clientAuthentication)
    {
        var certificate = GetFromCertificateStore(clientAuthentication.CertificateCommonName);

        _confidentalClient = ConfidentialClientApplicationBuilder
            .Create(clientAuthentication.ClientId)
            .WithAuthority(clientAuthentication.AuthorityUrl)
            .WithCertificate(certificate)
            .Build();
    }

    /// <summary>
    /// Create a new http client secured by the service certificate.
    /// </summary>
    /// <param name="endpoint">The endpoint you want to talk to.</param>
    /// <returns>An http client configured with base address and valid bearer token.</returns>
    public async Task<HttpClient> Create(FileServiceEndpoint endpoint)
    {
        var fileServiceAccessToken = await AcquireTokenAsync(endpoint.ApplicationId);
        var httpClient = new HttpClient
        {
            BaseAddress = endpoint.BaseAddress
        };
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", fileServiceAccessToken);
        return httpClient;
    }

    private async Task<string> AcquireTokenAsync(string resourceId)
    {
        return (await _confidentalClient
            .AcquireTokenForClient(new[] { $"{resourceId}/.default" })
            .ExecuteAsync())
            .AccessToken;
    }

    private static X509Certificate2 GetFromCertificateStore(string commonName)
    {
        using var certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        certStore.Open(OpenFlags.ReadOnly);
        var certs = certStore.Certificates.Find(X509FindType.FindBySubjectName, $"{commonName}", false)
            .OfType<X509Certificate2>()
            .Where(c => c.SubjectName.Name?.Contains($"CN={commonName}") ??
                        false) // Make sure it is actually the CN
            .ToArray();
        certStore.Close();

        if (certs.Length == 0) throw new Exception($"Cert '{commonName}' not found");

        // Return the newest certificate, enables certificate rotation.
        return certs.OrderByDescending(c => c.NotAfter).First();
    }
}
