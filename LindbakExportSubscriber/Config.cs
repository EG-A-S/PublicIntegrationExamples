using Microsoft.Extensions.Configuration;

namespace LindbakExportSubscriber
{
    public class Config
    {
        public string TenantServiceUrl { get; set; }
        public string TenantServiceCertificateName { get; set; }
        public string TenantServiceResourceId { get; set; }
        public string OutputDirectory { get; set; }

        public static Config Read(IConfigurationRoot build)
        {
            return new Config
            {
                TenantServiceUrl = build["TenantServiceUrl"],
                TenantServiceCertificateName = build["TenantServiceCertificateName"],
                TenantServiceResourceId = build["TenantServiceResourceId"],
                OutputDirectory = build["OutputDirectory"],
            };
        }
    }
}