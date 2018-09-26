namespace LindbakFileUpload
{
    /// <summary>
    /// Required configuration to talk to lindbak APIs.
    /// </summary>
    public class LindbakEndpointsSection
    {
        /// <summary>
        /// Common name part of the subject of the certificate provided by Lindbak.
        /// </summary>
        public string CertificateCommonName { get; set; }
        /// <summary>
        /// File service endpoint.
        /// </summary>
        public LindbakEndpoint FileService { get; set; } = new LindbakEndpoint();

        /// <summary>
        /// Verify that the section is valid.
        /// </summary>
        public bool IsValid =>
            !string.IsNullOrWhiteSpace(CertificateCommonName) &&
            FileService.IsValid;
    }
}