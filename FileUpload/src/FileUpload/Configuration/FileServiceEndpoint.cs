using System;

namespace FileUpload.Configuration;

/// <summary>
/// Configuration of a service endpoint.
/// </summary>
public class FileServiceEndpoint
{
    public string ApplicationId { get; set; }

    private Uri _baseAddress;
    public Uri BaseAddress
    {
        get => _baseAddress;
        set => _baseAddress = EnsurePathEndsWithSlashAndApi(value);
    }

    /// <summary>
    /// Verify that the section is valid.
    /// </summary>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(ApplicationId) &&
        (BaseAddress?.IsWellFormedOriginalString() ?? false);

    private static Uri EnsurePathEndsWithSlashAndApi(Uri value)
    {
        var uriBuilder = new UriBuilder(value);
        if (!uriBuilder.Path.EndsWith("/"))
        {
            uriBuilder.Path += "/";
        }
        if (!uriBuilder.Path.EndsWith("api/"))
        {
            uriBuilder.Path += "api/";
        }

        return uriBuilder.Uri;
    }
}
