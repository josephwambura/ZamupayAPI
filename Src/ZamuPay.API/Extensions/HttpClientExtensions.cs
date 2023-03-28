using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ZamuPay.API.Extensions
{
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> SendHttpClientRequestAsync(string baseUrl, HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken = default!)
        {
            HttpClient _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };

            // Send a POST request to the specified Uri as an asynchronous operation
            var response = await _httpClient.SendAsync(httpRequestMessage, cancellationToken: cancellationToken);

            return response;
        }
    }
}
