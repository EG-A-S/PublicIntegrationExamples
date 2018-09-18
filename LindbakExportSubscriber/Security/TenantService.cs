using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;

namespace LindbakExportSubscriber.Security
{
    public class TenantService
    {
        private readonly Uri _tenantServiceUrl;
        private readonly string _certificateCommonName;
        private readonly string _resourceId;
        private AuthenticationContext _authContext;

        public TenantService(Uri tenantServiceUrl, string certificateCommonName, string resourceId)
        {
            _tenantServiceUrl = tenantServiceUrl;
            if (_tenantServiceUrl.Segments.Length == 1)
                _tenantServiceUrl = new Uri(_tenantServiceUrl, "/api/external/integration/export/temporaryendpoints?integrationname=Default");
            _certificateCommonName = certificateCommonName;
            _resourceId = resourceId;
        }

        public async Task<ConnectionSecrets> GetSecrets()
        {
            var certificate = GetFromCertificateStore(_certificateCommonName);

            var (authUrl, clientId) = GetDetailsFromLindbakCertificate(certificate);

            if (_authContext == null)
                _authContext = new AuthenticationContext(authUrl, true);

            var setup = new
            {
                ClientId = clientId,
                X509Certificate = certificate,
                ServiceUrl = _tenantServiceUrl,
                ServiceResourceId = _resourceId
            };

            var token = (await _authContext.AcquireTokenAsync(setup.ServiceResourceId,
                new ClientAssertionCertificate(setup.ClientId, setup.X509Certificate))).AccessToken;
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var result = await httpClient.GetAsync(setup.ServiceUrl);
                var responseBody = await result.Content.ReadAsStringAsync();
                var secrets = JsonConvert.DeserializeObject<ConnectionSecrets>(responseBody);
                secrets.TimeUpdatedUtc = DateTimeOffset.UtcNow;
                return secrets;
            }
        }

        private static X509Certificate2 GetFromCertificateStore(string commonName)
        {
            using (var certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
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

        private static (string authUrl, string clientId) GetDetailsFromLindbakCertificate(X509Certificate2 certificate)
        {
            var certificateSubjectName = certificate?.SubjectName?.Name;

            if (string.IsNullOrWhiteSpace(certificateSubjectName))
            {
                throw new ArgumentException("Invalid certificate subject name");
            }

            var parts = certificateSubjectName.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            var error =
                $"Subject name does not contain clientId and authorityUrl, subject name is: {certificateSubjectName}";
            if (parts.Length < 3)
            {
                throw new ArgumentException(error);
            }

            var values = new System.Collections.Generic.Dictionary<string, string>();
            foreach (var part in parts)
            {
                if (string.IsNullOrWhiteSpace(part) || !part.Contains("="))
                    continue;
                var elementParts = part.Split(new[] {'='}, StringSplitOptions.RemoveEmptyEntries);
                values[elementParts[0].Trim()] = elementParts[1].Trim();
            }

            if (!values.ContainsKey("OU") || !values.ContainsKey("C"))
            {
                throw new ArgumentException(error);
            }

            return (values["C"], values["OU"]);
        }
    }
}