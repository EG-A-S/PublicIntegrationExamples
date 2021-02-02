using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using IdentityModel.Client;
using System.Web;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Formatting;

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
        private const string memberID = "{my_member_id}"; // Receipts belonging to this member will be fetched
        private const string tenantID = "{my_tenant_id}"; // Company tenant id
        private const string receiptServiceUrl = "https://lrsreceiptservice.azurewebsites.net";
        private const string authorityUrl = "https://lrsloyaltytest.azurewebsites.net/{my_tenant_id}/identity";
        private const string ClientId = "{my_client_id}";
        private const string ClientSecret = "{my_client_secret}";
        private const string ClientScope = "receiptapiclient";


        public ReceiptsController()
        {
        }


        private async Task<HttpClient> GetHttpClient()
        {
            var tokenResponse = await GetTokenAsync();

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.BaseAddress = new Uri(receiptServiceUrl);
            client.SetBearerToken(tokenResponse.AccessToken);

            return client;
        }


        private async Task<TokenResponse> GetTokenAsync()
        {
            var tokenClient = new TokenClient(
                authorityUrl + (authorityUrl.EndsWith("/") ? "" : "/") + "connect/token",
                ClientId,
                ClientSecret);

            return await tokenClient.RequestClientCredentialsAsync(ClientScope);
        }


        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Get([FromQuery] int top = -1)
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
            catch (Exception ex)
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
            catch (Exception e)
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
