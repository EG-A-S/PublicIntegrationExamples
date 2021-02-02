using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FileUpload
{
    /// <summary>
    /// Helpers for sending Json and multi part form data payloads.
    /// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Post the provided request data as Json and return response as Json.
        /// </summary>
        /// <typeparam name="TRequest">Type of the request.</typeparam>
        /// <typeparam name="TResponse">Type of the response</typeparam>
        /// <param name="httpClient">An initialized http client</param>
        /// <param name="requestUri">The full request uri</param>
        /// <param name="data">Request payload</param>
        /// <exception cref="T:System.Net.Http.HttpRequestException">The HTTP response is unsuccessful.</exception>
        /// /// <returns>Response as Json</returns>
        public static async Task<TResponse> PostAsJsonAsync<TRequest, TResponse>(
            this HttpClient httpClient, string requestUri, TRequest data)
        {
            var serializedRequest = JsonConvert.SerializeObject(data);
            var content = new StringContent(serializedRequest);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var httpResponseMessage = await httpClient.PostAsync(requestUri, content);
            httpResponseMessage.EnsureSuccessStatusCode();

            var serializedResponse = await httpResponseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TResponse>(serializedResponse);
        }

        /// <summary>
        /// Post the provided request data as compressed json lines using multi part form data.
        /// </summary>
        /// <typeparam name="T">Type of the request items</typeparam>
        /// <param name="httpClient">An initialized http client</param>
        /// <param name="requestUri">The full request uri</param>
        /// <param name="fileName">The name of the file</param>
        /// <param name="data">Request payload</param>
        /// <exception cref="T:System.Net.Http.HttpRequestException">The HTTP response is unsuccessful.</exception>
        /// <returns></returns>
        public static async Task UploadContentAsCompressedJsonLines<T>(
            this HttpClient httpClient,
            string requestUri,
            string fileName,
            IEnumerable<T> data)
        {
            using (var contentStream = new MemoryStream())
            {
                data.WriteAsCompressedJsonLinesTo(contentStream);
                var multipartFormDataContent = new MultipartFormDataContent
                {
                    { new StreamContent(contentStream), fileName, fileName }
                };
                multipartFormDataContent.Headers.ContentEncoding.Add("gzip");

                var httpResponseMessage = await httpClient.PostAsync(requestUri, multipartFormDataContent);
                httpResponseMessage.EnsureSuccessStatusCode();
            }
        }
    }
}