using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ZamuPay.API.Extensions
{
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> SendHttpClientRequestAsync(IHttpClientFactory _httpClientFactory, string baseUrl, HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken = default!)
        {
            using HttpClient _httpClient = _httpClientFactory.CreateClient();

            _httpClient.BaseAddress = new Uri(baseUrl);

            // Send a POST request to the specified Uri as an asynchronous operation
            var response = await _httpClient.SendAsync(httpRequestMessage, cancellationToken: cancellationToken);

            return response;
        }
    }
}
