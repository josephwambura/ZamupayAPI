using ZamuPay.API.DTOs;
using ZamuPay.API.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text;
using System.Threading;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using ZamuPay.API.Interfaces;

namespace ZamuPay.API.Services
{
    public class ZamupayService : IZamupayService
    {
        private readonly IHttpClientFactory _httpClientFactory = null!;
        private readonly IDistributedCache _distributedCache;
        private readonly IOptions<BaseUrlConfiguration> _baseUrlConfiguration;

        public ZamupayService(
            IHttpClientFactory httpClientFactory,
            IDistributedCache distributedCache,
            IOptions<BaseUrlConfiguration> baseUrlConfiguration)
            => (_httpClientFactory, _distributedCache, _baseUrlConfiguration) = (httpClientFactory, distributedCache, baseUrlConfiguration);

        #region Identity Server

        public async Task<ZamuApiResult<ZamupayIdentityServerAuthTokenDTO>> GetZamupayIdentityServerAuthTokenAsync(CancellationToken cancellationToken = default!)
        {
            var result = new ZamuApiResult<ZamupayIdentityServerAuthTokenDTO>();

            var errors = new List<ErrorDetailDTO>();

            var requestUrl = $"{_baseUrlConfiguration.Value.IdentityServerBase}connect/token";

            try
            {
                var redisPayload = await _distributedCache.GetAsync("ZamupayIdentityServerAuthToken", cancellationToken);

                if (redisPayload != null)
                {
                    return result.Success(Encoding.UTF8.GetString(redisPayload).FromJson<ZamupayIdentityServerAuthTokenDTO>());
                }

                // Create a dictionary of parameters
                var parameters = new Dictionary<string, string>
                {
                    { "client_id", _baseUrlConfiguration.Value.ClientId! },
                    { "client_secret", _baseUrlConfiguration.Value.ClientSecret! },
                    { "grant_type", _baseUrlConfiguration.Value.GrantType! },
                    { "scope", _baseUrlConfiguration.Value.Scope! }
                };

                // Create a content object with the parameters and the media type
                var content = new FormUrlEncodedContent(parameters);

                // Create a request message with the POST method and the Uri
                var request = new HttpRequestMessage(HttpMethod.Post, "connect/token")
                {
                    // Add the content to the request
                    Content = content
                };

                // Add some custom headers to the request without validation
                request.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");

                HttpResponseMessage response = await HttpClientExtensions.SendHttpClientRequestAsync(_httpClientFactory, _baseUrlConfiguration.Value.IdentityServerBase, request, cancellationToken);

                // Read the response content as a string
                var output = await response.Content.ReadAsStringAsync();

                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    var zamupayIdentityServerAuthToken = output.FromJson<ZamupayIdentityServerAuthTokenDTO>();

                    var options = new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(zamupayIdentityServerAuthToken.ExpiresIn)
                    };

                    await _distributedCache.SetAsync("ZamupayIdentityServerAuthToken", Encoding.UTF8.GetBytes(output), options, cancellationToken);

                    return result.Success(zamupayIdentityServerAuthToken);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        var error = output.FromJson<ErrorDetailDTO>();

                        errors.Add(error);

                        return result.Failed(errors);
                    }

                    errors.Add(new ErrorDetailDTO { Title = $"Reason phrase: {response.ReasonPhrase}", Status = (int)response.StatusCode });

                    return result.Failed(errors);
                }
            }
            catch (Exception ex)
            {
                errors.Add(new ErrorDetailDTO { Status = -1, Title = $"{requestUrl}>ErrorMessage {ex.Message}" });

                return result.Failed(errors);
            }
        }

        #endregion

        #region Routes

        public async Task<ZamuApiResult<ZamupayRoutesDTO>> GetZamupayRoutesAsync(int expirationTime, CancellationToken cancellationToken = default!)
        {
            var result = new ZamuApiResult<ZamupayRoutesDTO>();

            var errors = new List<ErrorDetailDTO>();

            var auth = await GetZamupayIdentityServerAuthTokenAsync(cancellationToken);

            if (!auth.Succeeded)
            {
                return result.Failed(auth.Errors);
            }

            var requestUrl = $"{_baseUrlConfiguration.Value.ApiBase}v1/transaction-routes/assigned-routes";

            try
            {
                var redisPayload = await _distributedCache.GetAsync("ZamupayRoutes");

                if (redisPayload != null)
                {
                    return result.Success(Encoding.UTF8.GetString(redisPayload).FromJson<ZamupayRoutesDTO>());
                }

                // Create a request message with the POST method and the Uri
                var request = new HttpRequestMessage(HttpMethod.Get, "v1/transaction-routes/assigned-routes");

                // Add some custom headers to the request without validation
                request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {auth.Items?.AccessToken}");

                HttpResponseMessage response = await HttpClientExtensions.SendHttpClientRequestAsync(_httpClientFactory, _baseUrlConfiguration.Value.ApiBase!, request, cancellationToken);

                // Read the response content as a string
                var output = await response.Content.ReadAsStringAsync();


                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    var zamupayRouteCollection = output.FromJson<ZamupayRoutesDTO>();

                    var options = new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationTime)
                    };

                    await _distributedCache.SetAsync("ZamupayRoutes", Encoding.UTF8.GetBytes(output), options, cancellationToken);

                    
                    return result.Success(zamupayRouteCollection);
                }
                else
                {

                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        var error = output.FromJson<ErrorDetailDTO>();

                        errors.Add(error);

                        return result.Failed(errors);
                    }

                    errors.Add(new ErrorDetailDTO { Title = $"Reason phrase: {response.ReasonPhrase}", Status = (int)response.StatusCode });

                    return result.Failed(errors);
                }
            }
            catch (Exception ex)
            {
                errors.Add(new ErrorDetailDTO { Status = -1, Title = $"{requestUrl}>ErrorMessage {ex.Message}" });

                return result.Failed(errors);
            }
        }

        public async Task<ZamuApiResult<RouteDTO>> GetZamupayRouteAsync(Guid id, int expirationTime, CancellationToken cancellationToken = default!)
        {
            var result = new ZamuApiResult<RouteDTO>();

            var zamupayRoutes = await GetZamupayRoutesAsync(expirationTime, cancellationToken);

            if (!zamupayRoutes.Succeeded)
            {
                return result.Failed(zamupayRoutes.Errors);
            }

            var routes = zamupayRoutes.Items;

            return result.Success(routes!.Routes?.FirstOrDefault(r => r.Id == id.ToString()));
        }

        public async Task<ZamuApiResult<IEnumerable<RouteDTO>>> GetZamupayRoutesByCategoryAsync(string category, int expirationTime, CancellationToken cancellationToken = default!)
        {
            var result = new ZamuApiResult<IEnumerable<RouteDTO>>();

            var zamupayRoutes = await GetZamupayRoutesAsync(expirationTime, cancellationToken);

            if (!zamupayRoutes.Succeeded)
            {
                return result.Failed(zamupayRoutes.Errors);
            }

            var routes = zamupayRoutes.Items;

            return result.Success(routes!.Routes.Where(route => route.Category == category));
        }

        public async Task<ZamuApiResult<IEnumerable<ChannelTypeDTO>>> GetZamupayRouteChannelTypesAsync(Guid id, int expirationTime, CancellationToken cancellationToken = default!)
        {
            var result = new ZamuApiResult<IEnumerable<ChannelTypeDTO>>();

            var zamupayRoute = await GetZamupayRouteAsync(id, expirationTime, cancellationToken);

            if (!zamupayRoute.Succeeded)
            {
                return result.Failed(zamupayRoute.Errors);
            }

            return result.Success(zamupayRoute.Items!.ChannelTypes);
        }

        #endregion

        #region Payment Orders

        public async Task<ZamuApiResult<PaymentOrderDTO>> GetPaymentOrderAsync(TransactionQueryModelDTO paymentQueryModel, CancellationToken cancellationToken = default!)
        {
            var result = new ZamuApiResult<PaymentOrderDTO>();

            var errors = new List<ErrorDetailDTO>();

            var auth = await GetZamupayIdentityServerAuthTokenAsync(cancellationToken);

            if (!auth.Succeeded)
            {
                return result.Failed(auth.Errors);
            }

            var requestUrl = $"{_baseUrlConfiguration.Value.ApiBase}v1/payment-order/check-status?Id={paymentQueryModel.Id}&IdType={paymentQueryModel.IdType}";

            try
            {
                // Create a request message with the POST method and the Uri
                var request = new HttpRequestMessage(HttpMethod.Get, $"v1/payment-order/check-status?Id={paymentQueryModel.Id}&IdType={paymentQueryModel.IdType}");

                // Add some custom headers to the request without validation
                request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {auth.Items?.AccessToken}");

                HttpResponseMessage response = await HttpClientExtensions.SendHttpClientRequestAsync(_httpClientFactory, _baseUrlConfiguration.Value.ApiBase!, request, cancellationToken);

                // Read the response content as a string
                var output = await response.Content.ReadAsStringAsync();

                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    var zamupayRouteCollection = output.FromJson<PaymentOrderDTO>();

                    return result.Success(zamupayRouteCollection);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        var error = output.FromJson<ErrorDetailDTO>();

                        errors.Add(error);

                        return result.Failed(errors);
                    }

                    errors.Add(new ErrorDetailDTO { Title = $"Reason phrase: {response.ReasonPhrase}", Status = (int)response.StatusCode });

                    return result.Failed(errors);
                }
            }
            catch (Exception ex)
            {
                errors.Add(new ErrorDetailDTO { Status = -1, Title = $"{requestUrl}>ErrorMessage {ex.Message}" });

                return result.Failed(errors);
            }
        }

        #endregion

        #region Airtime Purchases

        public Task<ZamuApiResult<AirtimePurchaseRequestDTO>> PostAirtimePurchaseRequestAsync(AirtimePurchaseRequestDTO paymentQueryModel, CancellationToken cancellationToken = default!)
        {
            throw new NotImplementedException();
        }

        public async Task<ZamuApiResult<PaymentOrderDTO>> GetAirtimePurchaseAsync(TransactionQueryModelDTO paymentQueryModel, CancellationToken cancellationToken = default!)
        {
            var result = new ZamuApiResult<PaymentOrderDTO>();

            var errors = new List<ErrorDetailDTO>();

            var auth = await GetZamupayIdentityServerAuthTokenAsync(cancellationToken);

            if (!auth.Succeeded)
                return result.Failed(auth.Errors);

            var requestUrl = $"{_baseUrlConfiguration.Value.ApiBase}v1/payment-order/check-status?Id={paymentQueryModel.Id}&IdType={paymentQueryModel.IdType}";

            try
            {
                // Create a request message with the POST method and the Uri
                var request = new HttpRequestMessage(HttpMethod.Get, $"v1/payment-order/check-status?Id={paymentQueryModel.Id}&IdType={paymentQueryModel.IdType}");

                // Add some custom headers to the request without validation
                request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {auth.Items?.AccessToken}");

                HttpResponseMessage response = await HttpClientExtensions.SendHttpClientRequestAsync(_httpClientFactory, _baseUrlConfiguration.Value.ApiBase!, request, cancellationToken);

                // Read the response content as a string
                var output = await response.Content.ReadAsStringAsync();

                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    var zamupayRouteCollection = output.FromJson<PaymentOrderDTO>();

                    return result.Success(zamupayRouteCollection);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        var error = output.FromJson<ErrorDetailDTO>();

                        errors.Add(error);

                        return result.Failed(errors);
                    }

                    errors.Add(new ErrorDetailDTO { Title = $"Reason phrase: {response.ReasonPhrase}", Status = (int)response.StatusCode });

                    return result.Failed(errors);
                }
            }
            catch (Exception ex)
            {
                errors.Add(new ErrorDetailDTO { Status = -1, Title = $"{requestUrl}>ErrorMessage {ex.Message}" });

                return result.Failed(errors);
            }
        }

        #endregion

        #region Bill Payments

        public async Task<ZamuApiResult<BillPaymentSuccessResponseDTO>> PostBillPaymentAsync(BillPaymentDTO billPaymentDTO, CancellationToken cancellationToken = default!)
        {
            var result = new ZamuApiResult<BillPaymentSuccessResponseDTO>();

            var errors = new List<ErrorDetailDTO>();

            var auth = await GetZamupayIdentityServerAuthTokenAsync(cancellationToken);

            if (!auth.Succeeded)
                return result.Failed(auth.Errors);

            var requestUrl = $"{_baseUrlConfiguration.Value.ApiBase}v1/bill-payments";

            try
            {
                
                // Create a request message with the POST method and the Uri
                var request = new HttpRequestMessage(HttpMethod.Post, "v1/bill-payments")
                {
                    // Add the content to the request
                    Content = new StringContent(billPaymentDTO.ToJson())
                };

                // Add some custom headers to the request without validation
                request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {auth.Items?.AccessToken}");
                request.Headers.TryAddWithoutValidation("Accept", "text/plain");

                HttpResponseMessage response = await HttpClientExtensions.SendHttpClientRequestAsync(_httpClientFactory, _baseUrlConfiguration.Value.ApiBase!, request, cancellationToken);

                // Read the response content as a string
                var output = await response.Content.ReadAsStringAsync();

                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    var zamupayRouteCollection = output.FromJson<BillPaymentSuccessResponseDTO>();

                    return result.Success(zamupayRouteCollection);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        var error = output.FromJson<ErrorDetailDTO>();

                        errors.Add(error);

                        return result.Failed(errors);
                    }

                    errors.Add(new ErrorDetailDTO { Title = $"Reason phrase: {response.ReasonPhrase}", Status = (int)response.StatusCode });

                    return result.Failed(errors);
                }
            }
            catch (Exception ex)
            {
                errors.Add(new ErrorDetailDTO { Status = -1, Title = $"{requestUrl}>ErrorMessage {ex.Message}" });

                return result.Failed(errors);
            }
        }

        public async Task<ZamuApiResult<BillPaymentResultDTO>> GetBillPaymentAsync(TransactionQueryModelDTO paymentQueryModel, CancellationToken cancellationToken = default!)
        {
            var result = new ZamuApiResult<BillPaymentResultDTO>();

            var errors = new List<ErrorDetailDTO>();

            var auth = await this.GetZamupayIdentityServerAuthTokenAsync(cancellationToken);

            if (!auth.Succeeded)
                return result.Failed(auth.Errors);

            var requestUrl = $"{_baseUrlConfiguration.Value.ApiBase}v1/bill-payments?{paymentQueryModel.IdType}={paymentQueryModel.Id}";

            try
            {
                // Create a request message with the POST method and the Uri
                var request = new HttpRequestMessage(HttpMethod.Get, $"v1/bill-payments?{paymentQueryModel.IdType}={paymentQueryModel.Id}");

                // Add some custom headers to the request without validation
                request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {auth.Items?.AccessToken}");

                HttpResponseMessage response = await HttpClientExtensions.SendHttpClientRequestAsync(_httpClientFactory, _baseUrlConfiguration.Value.ApiBase!, request, cancellationToken);

                // Read the response content as a string
                var output = await response.Content.ReadAsStringAsync();

                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    var zamupayRouteCollection = output.FromJson<BillPaymentResultDTO>();

                    return result.Success(zamupayRouteCollection);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        var error = output.FromJson<ErrorDetailDTO>();

                        errors.Add(error);

                        return result.Failed(errors);
                    }

                    errors.Add(new ErrorDetailDTO { Title = $"Reason phrase: {response.ReasonPhrase}", Status = (int)response.StatusCode });

                    return result.Failed(errors);
                }
            }
            catch (Exception ex)
            {
                errors.Add(new ErrorDetailDTO { Status = -1, Title = $"{requestUrl}>ErrorMessage {ex.Message}" });

                return result.Failed(errors);
            }
        }

        #endregion
    }
}