using System;

namespace ExportSubscriber.Security
{
    /// <summary>
    /// Temporary secrets as returned from TenantService
    /// </summary>
    public class ConnectionSecrets
    {
        public string serviceBusSubscriptionConnectionString { get; set; }
        public Uri[] blobBaseUrlsWithToken { get; set; }
        public DateTimeOffset TimeUpdatedUtc { get; set; }
        public string serviceBusSubscriptionName { get; set; }
    }
}
