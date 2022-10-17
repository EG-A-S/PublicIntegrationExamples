using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;

namespace ReceiptServiceDotnetCore.Controllers
{
    public class ReceiptIdentificator
    {
        public int StoreNumber { get; set; }
        public int PosNumber { get; set; }
        public int SequenceNumber { get; set; }
        public string TenantId { get; set; }
    }

    public class Receipt
    {
        public string Id { get; set; }
        public string Nsid { get; set; }
        public ReceiptIdentificator Cid { get; set; }
        public string PartitionKey { get; set; }
        public decimal TotalAmount { get; set; }
        public string LoyaltyId { get; set; }
        public DateTimeOffset EndDateTimeUtc { get; set; }
        public string StoreName { get; set; }
        public string TimeZone { get; set; }
        public ReceiptFlags Flags { get; set; }
    }

    public class ReceiptFlags
    {
        public bool HasWarranty { get; set; }
        public bool IsDeleted { get; set; }
    }

    internal class ODataResponse<T>
    {
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }
        [JsonProperty("value")]
        public T[] Value { get; set; }
    }

    public struct ReceiptDto
    {
        public string Id { get; set; }
        public string Nsid { get; set; }
        public decimal TotalAmount { get; set; }
        public long EndDateTimeUtc { get; set; }
        public string StoreName { get; set; }

        public static ReceiptDto FromReceipt(Receipt receipt)
        {
            return new ReceiptDto
            {
                Id = receipt.Id,
                Nsid = receipt.Nsid,
                TotalAmount = receipt.TotalAmount,
                EndDateTimeUtc = receipt.EndDateTimeUtc.ToUnixTimeMilliseconds(),
                StoreName = receipt.StoreName
            };
        }
    }


    [Route("api/receipts")]
    public class ReceiptsController : Controller
    {
        private const string memberID = "123"; // Receipts belonging to this member will be fetched
        private const string receiptServiceUrl = "https://receiptservice.egretail-dev.cloud";
        private const string receiptServiceResourceId = "568312bf-50b9-4027-82e2-bd8235acf65f";
        private const string authorityUrl = "https://login.microsoftonline.com/f90c551f-145b-45fe-9610-c99bc6a0a464";
        private const string ClientId = "";
        private const string ClientSecret = "";

        public ReceiptsController()
        {
        }


        private async Task<HttpClient> GetHttpClient()
        {
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(ClientId)
                .WithClientSecret(ClientSecret)
                .WithAuthority(authorityUrl)
                .Build();

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.BaseAddress = new Uri(receiptServiceUrl);

            string[] scopes = new string[] { $"{receiptServiceResourceId}/.default" };
            var accessToken = await app.AcquireTokenForClient(scopes).ExecuteAsync();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.AccessToken);

            return client;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Get([FromQuery] int top = 10)
        {
            var query = $"?$filter=loyaltyId eq '{memberID}'";
            query += "&$orderBy=endDateTimeUtc desc";
            query += top > 0 ? $"&$top={top}" : "";

            try
            {
                var webclient = await GetHttpClient();
                var response = await webclient.GetAsync("/odata/Receipt" + query);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsAsync<ODataResponse<Receipt>>();

                if (result == null)
                {
                    var ex = new Exception("Got null on reading from receipt service response");
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }

                var receipts = (from receipt in result.Value
                                where receipt.Flags != null && !receipt.Flags.IsDeleted
                                select ReceiptDto.FromReceipt(receipt)).ToList();

                return Ok(receipts);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        [HttpGet]
        [Route("render")]
        public async Task<HttpResponseMessage> Render([FromQuery]string receiptId, [FromQuery]string type)
        {
            try
            {
                var webclient = await GetHttpClient();

                var requestUrl = $"/api/receipt/{receiptId}/rendering/{type}";

                var responseWithHeadersOnly = await webclient.GetAsync(requestUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

                responseWithHeadersOnly.EnsureSuccessStatusCode();

                var result = new HttpResponseMessage();
                result.Content = CopyContentStream(responseWithHeadersOnly);
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline") // "attachment"(download) "inline"(in browser)
                {
                    FileName = $"receipt_{receiptId}.pdf"
                };
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

                return result;
            }
            catch
            {
                return new HttpResponseMessage {StatusCode = System.Net.HttpStatusCode.NotFound};
            }
        }


        private static PushStreamContent CopyContentStream(HttpResponseMessage sourceContent)
        {
            return new PushStreamContent(async (stream, content, context) =>
            {
                using (stream)
                using (var sourceStream = await sourceContent.Content.ReadAsStreamAsync())
                {
                    await sourceStream.CopyToAsync(stream);
                }
            });
        }

    }

}
