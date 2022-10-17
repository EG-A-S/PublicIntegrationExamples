using System;

namespace FileUpload.Configuration;

public class ClientAuthentication
{
    /// <summary>
    /// Common name part of the subject of the certificate provided by EG Retail.
    /// </summary>
    public string CertificateCommonName { get; set; }

    public string ClientId { get; set; }

    public string AuthorityUrl { get; set; }

    public bool IsValid =>
            !string.IsNullOrWhiteSpace(CertificateCommonName) &&
            Uri.TryCreate(AuthorityUrl, UriKind.Absolute, out var _) &&
            Guid.TryParse(ClientId, out var _);
}