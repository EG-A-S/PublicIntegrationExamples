using System;

namespace LindbakExportSubscriber.Security
{
    public class ConnectionSecrets
    {
        public string ServiceBusConnectionstring { get; set; }
        public string BlobSasToken { get; set; }
        public DateTimeOffset TimeUpdatedUtc { get; set; }
        public string ServiceBusSubscriptionName { get; set; }
    }
}
