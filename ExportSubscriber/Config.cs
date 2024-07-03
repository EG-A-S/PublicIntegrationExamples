using Microsoft.Extensions.Configuration;

namespace ExportSubscriber
{
    public class Config
    {
        public string TenantServiceUrl { get; set; }
        public string TenantServiceCertificateName { get; set; }
        public string TenantServiceResourceId { get; set; }
        public string AuthorityUrl { get; set; }
        public string ClientId { get; set; }
        public string OutputDirectory { get; set; }
        public string IntegrationPartnerName { get; private set; }
        public string UseNewSBConnectionStringFormat  {get; set;}

        public static Config Read(IConfigurationRoot build)
        {
            return new Config
            {
                TenantServiceUrl = build["TenantServiceUrl"],
                TenantServiceCertificateName = build["TenantServiceCertificateName"],
                TenantServiceResourceId = build["TenantServiceResourceId"],
                AuthorityUrl = build["AuthorityUrl"],
                ClientId = build["ClientId"],
                IntegrationPartnerName = build["IntegrationPartnerName"],
                OutputDirectory = build["OutputDirectory"],
                UseNewSBConnectionStringFormat = build["UseNewSBConnectionStringFormat"]
            };
        }
    }
}
