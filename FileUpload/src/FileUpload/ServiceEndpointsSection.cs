namespace FileUpload
{
    /// <summary>
    /// Required configuration to talk to service APIs.
    /// </summary>
    public class ServiceEndpointsSection
    {
        /// <summary>
        /// Common name part of the subject of the certificate provided by EG Retail.
        /// </summary>
        public string CertificateCommonName { get; set; }
        /// <summary>
        /// File service endpoint.
        /// </summary>
        public ServiceEndpoint FileService { get; set; } = new ServiceEndpoint();

        /// <summary>
        /// Verify that the section is valid.
        /// </summary>
        public bool IsValid =>
            !string.IsNullOrWhiteSpace(CertificateCommonName) &&
            FileService.IsValid;
    }
}
