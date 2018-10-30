using System;

namespace LindbakFileUpload
{
    /// <summary>
    /// Configuration of a Lindbak endpoint.
    /// </summary>
    public class LindbakEndpoint
    {
        private Uri _baseAddress;
        /// <summary>
        /// Azure AD application Id. Also called resource id.
        /// </summary>
        public string ApplicationId { get; set; }

        /// <summary>
        /// The base url to the endpoint.
        /// </summary>
        public Uri BaseAddress
        {
            get => _baseAddress;
            set => _baseAddress = EnsurePathEndsWithSlash(value);
        }

        /// <summary>
        /// Verify that the section is valid.
        /// </summary>
        public bool IsValid => 
            !string.IsNullOrWhiteSpace(ApplicationId) &&
            (BaseAddress?.IsWellFormedOriginalString() ?? false);

        private static Uri EnsurePathEndsWithSlash(Uri value)
        {
            var uriBuilder = new UriBuilder(value);
            if (!uriBuilder.Path.EndsWith("/"))
            {
                uriBuilder.Path += "/";
            }

            return uriBuilder.Uri;
        }
    }
}