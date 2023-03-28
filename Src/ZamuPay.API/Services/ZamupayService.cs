using ZamuPay.API.DTOs;
using ZamuPay.API.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using RestSharp;
using System.Text;
using System.Threading;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace ZamuPay.API.Services
{
    public class ZamupayService : IZamupayService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IOptions<BaseUrlConfiguration> _baseUrlConfiguration;

        public ZamupayService(
            IDistributedCache distributedCache,
            IOptions<BaseUrlConfiguration> baseUrlConfiguration)
        {
            _distributedCache = distributedCache;
            _baseUrlConfiguration = baseUrlConfiguration;
        }

        #region Identity Server

        public async Task<(ZamupayIdentityServerAuthTokenDTO?, ErrorDetailDTO)> GetZamupayIdentityServerAuthTokenAsync(CancellationToken cancellationToken)
        {
            var requestUrl = $"{_baseUrlConfiguration.Value.IdentityServerBase}connect/token";

            try
            {
                var redisPayload = await _distributedCache.GetAsync("ZamupayIdentityServerAuthToken");

                if (redisPayload != null)
                {
                    return (Encoding.UTF8.GetString(redisPayload).FromJson<ZamupayIdentityServerAuthTokenDTO>(), null);
                }

                var client = new RestClient(requestUrl);
                var request = new RestRequest(requestUrl, Method.Post);

                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddParameter("client_id", _baseUrlConfiguration.Value.ClientId);
                request.AddParameter("client_secret", _baseUrlConfiguration.Value.ClientSecret);
                request.AddParameter("grant_type", _baseUrlConfiguration.Value.GrantType);
                request.AddParameter("scope", _baseUrlConfiguration.Value.Scope);

                RestResponse response = await client.ExecuteAsync(request);

                var output = response.Content;

                if (response.IsSuccessful && !string.IsNullOrWhiteSpace(output))
                {
                    var zamupayIdentityServerAuthToken = output.FromJson<ZamupayIdentityServerAuthTokenDTO>();

                    var options = new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(zamupayIdentityServerAuthToken.ExpiresIn)
                    };

                    await _distributedCache.SetAsync("ZamupayIdentityServerAuthToken", Encoding.UTF8.GetBytes(output), options, cancellationToken);

                    return (zamupayIdentityServerAuthToken, null);
                }
            }
            catch (Exception ex)
            {
                return (null, new ErrorDetailDTO { Status = -1, Title = $"{requestUrl}>ErrorMessage {ex.Message}" });
            }

            return (null, null);
        }

        #endregion

        #region Routes

        public async Task<(ZamupayRoutesDTO?, ErrorDetailDTO)> GetZamupayRoutesAsync(int expirationTime, CancellationToken cancellationToken)
        {
            var auth = await this.GetZamupayIdentityServerAuthTokenAsync(CancellationToken.None);

            if (auth.Item2 != null)
                return (null, auth.Item2);

            var requestUrl = $"{_baseUrlConfiguration.Value.ApiBase}v1/transaction-routes/assigned-routes";

            try
            {
                var redisPayload = await _distributedCache.GetAsync("ZamupayRoutes");

                if (redisPayload != null)
                {
                    return (Encoding.UTF8.GetString(redisPayload).FromJson<ZamupayRoutesDTO>(), null);
                }

                var client = new RestClient(requestUrl);
                var request = new RestRequest(requestUrl, Method.Get);

                request.AddHeader("Authorization", $"Bearer {auth.Item1?.AccessToken}");

                RestResponse response = await client.ExecuteAsync(request);

                var output = response.Content;

                if (response.IsSuccessful && !string.IsNullOrWhiteSpace(output))
                {
                    var zamupayRouteCollection = output.FromJson<ZamupayRoutesDTO>();

                    var options = new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationTime)
                    };

                    await _distributedCache.SetAsync("ZamupayRoutes", Encoding.UTF8.GetBytes(output), options, cancellationToken);

                    return (zamupayRouteCollection, null);
                }

                if (!string.IsNullOrWhiteSpace(output))
                {
                    var error = output.FromJson<ErrorDetailDTO>();

                    return (null, error);
                }
            }
            catch (Exception ex)
            {
                return (null, new ErrorDetailDTO { Status = -1, Title = $"{requestUrl}>ErrorMessage {ex.Message}" });
            }

            return (null, null);
        }

        public async Task<(RouteDTO?, ErrorDetailDTO)> GetZamupayRouteAsync(Guid id, int expirationTime, CancellationToken cancellationToken)
        {
            var zamupayRoutes = await this.GetZamupayRoutesAsync(expirationTime, cancellationToken);

            if (zamupayRoutes.Item2 != null)
            {
                return (null, zamupayRoutes.Item2);
            }

            return (zamupayRoutes.Item1?.Routes.ToList().FirstOrDefault(r => r.Id == id.ToString()), null);
        }

        public async Task<(IEnumerable<RouteDTO>?, ErrorDetailDTO)> GetZamupayRoutesByCategoryAsync(string category, int expirationTime, CancellationToken cancellationToken)
        {
            var zamupayRoutes = await this.GetZamupayRoutesAsync(expirationTime, cancellationToken);

            if (zamupayRoutes.Item2 != null)
            {
                return (null, zamupayRoutes.Item2);
            }

            return (zamupayRoutes.Item1?.Routes.ToList().Where(r => r.Category == category).ToList(), null);
        }

        public async Task<(IEnumerable<ChannelTypeDTO>?, ErrorDetailDTO)> GetZamupayRouteChannelTypesAsync(Guid id, int expirationTime, CancellationToken cancellationToken)
        {
            var zamupayRoute = await this.GetZamupayRouteAsync(id, expirationTime, cancellationToken);

            if (zamupayRoute.Item2 != null)
            {
                return (null, zamupayRoute.Item2);
            }

            return (zamupayRoute.Item1?.ChannelTypes, null);
        }

        #endregion

        #region Payment Orders

        public async Task<(PaymentOrderDTO?, object)> GetPaymentOrderAsync(TransactionQueryModelDTO paymentQueryModel, CancellationToken cancellationToken)
        {
            var auth = await this.GetZamupayIdentityServerAuthTokenAsync(CancellationToken.None);

            if (auth.Item2 != null)
                return (null, auth.Item2);

            var requestUrl = $"{_baseUrlConfiguration.Value.ApiBase}v1/payment-order/check-status?Id={paymentQueryModel.Id}&IdType={paymentQueryModel.IdType}";

            try
            {
                var client = new RestClient(requestUrl);
                var request = new RestRequest(requestUrl, Method.Get);

                request.AddHeader("Authorization", $"Bearer {auth.Item1?.AccessToken}");

                RestResponse response = await client.ExecuteAsync(request);

                var output = response.Content;

                if (!string.IsNullOrWhiteSpace(output))
                {
                    if (response.IsSuccessful)
                    {
                        switch (response.StatusCode)
                        {
                            case System.Net.HttpStatusCode.Accepted:
                                break;
                            case System.Net.HttpStatusCode.AlreadyReported:
                                break;
                            case System.Net.HttpStatusCode.Ambiguous:
                                break;
                            case System.Net.HttpStatusCode.BadGateway:
                                break;
                            case System.Net.HttpStatusCode.BadRequest:

                                //var zamupayPaymentOrderQueryErrors = output.FromJson<PaymentFailedWithErrorsResponseDTO>();

                                //return (null, new ErrorDetailDTO
                                //{
                                //    Status = (int)System.Net.HttpStatusCode.BadRequest,
                                //    Title = zamupayPaymentOrderQueryErrors.Message,
                                //    TraceId = zamupayPaymentOrderQueryErrors.Id
                                //});

                                return (null, output.FromJson<PaymentFailedWithErrorsResponseDTO>());

                            //break;
                            case System.Net.HttpStatusCode.Conflict:
                                break;
                            case System.Net.HttpStatusCode.Continue:
                                break;
                            case System.Net.HttpStatusCode.Created:
                                break;
                            case System.Net.HttpStatusCode.EarlyHints:
                                break;
                            case System.Net.HttpStatusCode.ExpectationFailed:
                                break;
                            case System.Net.HttpStatusCode.FailedDependency:
                                break;
                            case System.Net.HttpStatusCode.Forbidden:

                                //var zamupayPaymentOrderNotFound = output.FromJson<PaymentOrderNotFoundDTO>();

                                //return (null, new ErrorDetailDTO
                                //{
                                //    Status = (int)System.Net.HttpStatusCode.NotFound,
                                //    Title = zamupayPaymentOrderNotFound.Message,
                                //    TraceId = zamupayPaymentOrderNotFound.Id
                                //});

                                return (null, output.FromJson<PaymentOrderNotFoundDTO>());

                                //break;
                            case System.Net.HttpStatusCode.Found:
                                break;
                            case System.Net.HttpStatusCode.GatewayTimeout:
                                break;
                            case System.Net.HttpStatusCode.Gone:
                                break;
                            case System.Net.HttpStatusCode.HttpVersionNotSupported:
                                break;
                            case System.Net.HttpStatusCode.IMUsed:
                                break;
                            case System.Net.HttpStatusCode.InsufficientStorage:
                                break;
                            case System.Net.HttpStatusCode.InternalServerError:
                                break;
                            case System.Net.HttpStatusCode.LengthRequired:
                                break;
                            case System.Net.HttpStatusCode.Locked:
                                break;
                            case System.Net.HttpStatusCode.LoopDetected:
                                break;
                            case System.Net.HttpStatusCode.MethodNotAllowed:
                                break;
                            case System.Net.HttpStatusCode.MisdirectedRequest:
                                break;
                            case System.Net.HttpStatusCode.Moved:
                                break;
                            //case System.Net.HttpStatusCode.MovedPermanently:
                            //    break;
                            //case System.Net.HttpStatusCode.MultipleChoices:
                            //    break;
                            case System.Net.HttpStatusCode.MultiStatus:
                                break;
                            case System.Net.HttpStatusCode.NetworkAuthenticationRequired:
                                break;
                            case System.Net.HttpStatusCode.NoContent:
                                break;
                            case System.Net.HttpStatusCode.NonAuthoritativeInformation:
                                break;
                            case System.Net.HttpStatusCode.NotAcceptable:
                                break;
                            case System.Net.HttpStatusCode.NotExtended:
                                break;
                            case System.Net.HttpStatusCode.NotFound:

                                //var zamupayPaymentOrderNotFound = output.FromJson<PaymentOrderNotFoundDTO>();

                                //return (null, new ErrorDetailDTO
                                //{
                                //    Status = (int)System.Net.HttpStatusCode.NotFound,
                                //    Title = zamupayPaymentOrderNotFound.Message,
                                //    TraceId = zamupayPaymentOrderNotFound.Id
                                //});

                                return (null, output.FromJson<PaymentOrderNotFoundDTO>());

                                //break;
                            case System.Net.HttpStatusCode.NotImplemented:
                                break;
                            case System.Net.HttpStatusCode.NotModified:
                                break;
                            case System.Net.HttpStatusCode.OK:

                                return (output.FromJson<PaymentOrderDTO>(), null);

                                //break;
                            case System.Net.HttpStatusCode.PartialContent:
                                break;
                            case System.Net.HttpStatusCode.PaymentRequired:
                                break;
                            case System.Net.HttpStatusCode.PermanentRedirect:
                                break;
                            case System.Net.HttpStatusCode.PreconditionFailed:
                                break;
                            case System.Net.HttpStatusCode.PreconditionRequired:
                                break;
                            case System.Net.HttpStatusCode.Processing:
                                break;
                            case System.Net.HttpStatusCode.ProxyAuthenticationRequired:
                                break;
                            //case System.Net.HttpStatusCode.Redirect:
                            //    break;
                            case System.Net.HttpStatusCode.RedirectKeepVerb:
                                break;
                            case System.Net.HttpStatusCode.RedirectMethod:
                                break;
                            case System.Net.HttpStatusCode.RequestedRangeNotSatisfiable:
                                break;
                            case System.Net.HttpStatusCode.RequestEntityTooLarge:
                                break;
                            case System.Net.HttpStatusCode.RequestHeaderFieldsTooLarge:
                                break;
                            case System.Net.HttpStatusCode.RequestTimeout:
                                break;
                            case System.Net.HttpStatusCode.RequestUriTooLong:
                                break;
                            case System.Net.HttpStatusCode.ResetContent:
                                break;
                            //case System.Net.HttpStatusCode.SeeOther:
                            //    break;
                            case System.Net.HttpStatusCode.ServiceUnavailable:
                                break;
                            case System.Net.HttpStatusCode.SwitchingProtocols:
                                break;
                            //case System.Net.HttpStatusCode.TemporaryRedirect:
                            //    break;
                            case System.Net.HttpStatusCode.TooManyRequests:
                                break;
                            case System.Net.HttpStatusCode.Unauthorized:

                                //var zamupayPaymentOrderNotFound = output.FromJson<PaymentOrderNotFoundDTO>();

                                //return (null, new ErrorDetailDTO
                                //{
                                //    Status = (int)System.Net.HttpStatusCode.NotFound,
                                //    Title = zamupayPaymentOrderNotFound.Message,
                                //    TraceId = zamupayPaymentOrderNotFound.Id
                                //});

                                return (null, output.FromJson<PaymentOrderNotFoundDTO>());

                                //break;
                            case System.Net.HttpStatusCode.UnavailableForLegalReasons:
                                break;
                            case System.Net.HttpStatusCode.UnprocessableEntity:
                                break;
                            case System.Net.HttpStatusCode.UnsupportedMediaType:
                                break;
                            case System.Net.HttpStatusCode.Unused:
                                break;
                            case System.Net.HttpStatusCode.UpgradeRequired:
                                break;
                            case System.Net.HttpStatusCode.UseProxy:
                                break;
                            case System.Net.HttpStatusCode.VariantAlsoNegotiates:
                                break;
                            default:
                                break;
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(output))
                {
                    var error = output.FromJson<ErrorDetailDTO>();

                    return (null, error);
                }
            }
            catch (Exception ex)
            {
                return (null, new ErrorDetailDTO { Status = -1, Title = $"{requestUrl}>ErrorMessage {ex.Message}" });
            }

            return (null, null);
        }

        #endregion

        #region Airtime Purchases

        public Task<(AirtimePurchaseRequestDTO?, object)> PostAirtimePurchaseRequestAsync(AirtimePurchaseRequestDTO paymentQueryModel, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<(PaymentOrderDTO?, object)> GetAirtimePurchaseAsync(TransactionQueryModelDTO paymentQueryModel, CancellationToken cancellationToken)
        {
            var auth = await this.GetZamupayIdentityServerAuthTokenAsync(CancellationToken.None);

            if (auth.Item2 != null)
                return (null, auth.Item2);

            var requestUrl = $"{_baseUrlConfiguration.Value.ApiBase}v1/payment-order/check-status?Id={paymentQueryModel.Id}&IdType={paymentQueryModel.IdType}";

            try
            {
                var client = new RestClient(requestUrl);
                var request = new RestRequest(requestUrl, Method.Get);

                request.AddHeader("Authorization", $"Bearer {auth.Item1?.AccessToken}");

                RestResponse response = await client.ExecuteAsync(request);

                var output = response.Content;

                if (!string.IsNullOrWhiteSpace(output))
                {
                    if (response.IsSuccessful)
                    {
                        switch (response.StatusCode)
                        {
                            case System.Net.HttpStatusCode.Accepted:
                                break;
                            case System.Net.HttpStatusCode.AlreadyReported:
                                break;
                            case System.Net.HttpStatusCode.Ambiguous:
                                break;
                            case System.Net.HttpStatusCode.BadGateway:
                                break;
                            case System.Net.HttpStatusCode.BadRequest:

                                //var zamupayPaymentOrderQueryErrors = output.FromJson<PaymentFailedWithErrorsResponseDTO>();

                                //return (null, new ErrorDetailDTO
                                //{
                                //    Status = (int)System.Net.HttpStatusCode.BadRequest,
                                //    Title = zamupayPaymentOrderQueryErrors.Message,
                                //    TraceId = zamupayPaymentOrderQueryErrors.Id
                                //});

                                return (null, output.FromJson<PaymentFailedWithErrorsResponseDTO>());

                            //break;
                            case System.Net.HttpStatusCode.Conflict:
                                break;
                            case System.Net.HttpStatusCode.Continue:
                                break;
                            case System.Net.HttpStatusCode.Created:
                                break;
                            case System.Net.HttpStatusCode.EarlyHints:
                                break;
                            case System.Net.HttpStatusCode.ExpectationFailed:
                                break;
                            case System.Net.HttpStatusCode.FailedDependency:
                                break;
                            case System.Net.HttpStatusCode.Forbidden:

                                //var zamupayPaymentOrderNotFound = output.FromJson<PaymentOrderNotFoundDTO>();

                                //return (null, new ErrorDetailDTO
                                //{
                                //    Status = (int)System.Net.HttpStatusCode.NotFound,
                                //    Title = zamupayPaymentOrderNotFound.Message,
                                //    TraceId = zamupayPaymentOrderNotFound.Id
                                //});

                                return (null, output.FromJson<PaymentOrderNotFoundDTO>());

                            //break;
                            case System.Net.HttpStatusCode.Found:
                                break;
                            case System.Net.HttpStatusCode.GatewayTimeout:
                                break;
                            case System.Net.HttpStatusCode.Gone:
                                break;
                            case System.Net.HttpStatusCode.HttpVersionNotSupported:
                                break;
                            case System.Net.HttpStatusCode.IMUsed:
                                break;
                            case System.Net.HttpStatusCode.InsufficientStorage:
                                break;
                            case System.Net.HttpStatusCode.InternalServerError:
                                break;
                            case System.Net.HttpStatusCode.LengthRequired:
                                break;
                            case System.Net.HttpStatusCode.Locked:
                                break;
                            case System.Net.HttpStatusCode.LoopDetected:
                                break;
                            case System.Net.HttpStatusCode.MethodNotAllowed:
                                break;
                            case System.Net.HttpStatusCode.MisdirectedRequest:
                                break;
                            case System.Net.HttpStatusCode.Moved:
                                break;
                            //case System.Net.HttpStatusCode.MovedPermanently:
                            //    break;
                            //case System.Net.HttpStatusCode.MultipleChoices:
                            //    break;
                            case System.Net.HttpStatusCode.MultiStatus:
                                break;
                            case System.Net.HttpStatusCode.NetworkAuthenticationRequired:
                                break;
                            case System.Net.HttpStatusCode.NoContent:
                                break;
                            case System.Net.HttpStatusCode.NonAuthoritativeInformation:
                                break;
                            case System.Net.HttpStatusCode.NotAcceptable:
                                break;
                            case System.Net.HttpStatusCode.NotExtended:
                                break;
                            case System.Net.HttpStatusCode.NotFound:

                                //var zamupayPaymentOrderNotFound = output.FromJson<PaymentOrderNotFoundDTO>();

                                //return (null, new ErrorDetailDTO
                                //{
                                //    Status = (int)System.Net.HttpStatusCode.NotFound,
                                //    Title = zamupayPaymentOrderNotFound.Message,
                                //    TraceId = zamupayPaymentOrderNotFound.Id
                                //});

                                return (null, output.FromJson<PaymentOrderNotFoundDTO>());

                            //break;
                            case System.Net.HttpStatusCode.NotImplemented:
                                break;
                            case System.Net.HttpStatusCode.NotModified:
                                break;
                            case System.Net.HttpStatusCode.OK:

                                return (output.FromJson<PaymentOrderDTO>(), null);

                            //break;
                            case System.Net.HttpStatusCode.PartialContent:
                                break;
                            case System.Net.HttpStatusCode.PaymentRequired:
                                break;
                            case System.Net.HttpStatusCode.PermanentRedirect:
                                break;
                            case System.Net.HttpStatusCode.PreconditionFailed:
                                break;
                            case System.Net.HttpStatusCode.PreconditionRequired:
                                break;
                            case System.Net.HttpStatusCode.Processing:
                                break;
                            case System.Net.HttpStatusCode.ProxyAuthenticationRequired:
                                break;
                            //case System.Net.HttpStatusCode.Redirect:
                            //    break;
                            case System.Net.HttpStatusCode.RedirectKeepVerb:
                                break;
                            case System.Net.HttpStatusCode.RedirectMethod:
                                break;
                            case System.Net.HttpStatusCode.RequestedRangeNotSatisfiable:
                                break;
                            case System.Net.HttpStatusCode.RequestEntityTooLarge:
                                break;
                            case System.Net.HttpStatusCode.RequestHeaderFieldsTooLarge:
                                break;
                            case System.Net.HttpStatusCode.RequestTimeout:
                                break;
                            case System.Net.HttpStatusCode.RequestUriTooLong:
                                break;
                            case System.Net.HttpStatusCode.ResetContent:
                                break;
                            //case System.Net.HttpStatusCode.SeeOther:
                            //    break;
                            case System.Net.HttpStatusCode.ServiceUnavailable:
                                break;
                            case System.Net.HttpStatusCode.SwitchingProtocols:
                                break;
                            //case System.Net.HttpStatusCode.TemporaryRedirect:
                            //    break;
                            case System.Net.HttpStatusCode.TooManyRequests:
                                break;
                            case System.Net.HttpStatusCode.Unauthorized:

                                //var zamupayPaymentOrderNotFound = output.FromJson<PaymentOrderNotFoundDTO>();

                                //return (null, new ErrorDetailDTO
                                //{
                                //    Status = (int)System.Net.HttpStatusCode.NotFound,
                                //    Title = zamupayPaymentOrderNotFound.Message,
                                //    TraceId = zamupayPaymentOrderNotFound.Id
                                //});

                                return (null, output.FromJson<PaymentOrderNotFoundDTO>());

                            //break;
                            case System.Net.HttpStatusCode.UnavailableForLegalReasons:
                                break;
                            case System.Net.HttpStatusCode.UnprocessableEntity:
                                break;
                            case System.Net.HttpStatusCode.UnsupportedMediaType:
                                break;
                            case System.Net.HttpStatusCode.Unused:
                                break;
                            case System.Net.HttpStatusCode.UpgradeRequired:
                                break;
                            case System.Net.HttpStatusCode.UseProxy:
                                break;
                            case System.Net.HttpStatusCode.VariantAlsoNegotiates:
                                break;
                            default:
                                break;
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(output))
                {
                    var error = output.FromJson<ErrorDetailDTO>();

                    return (null, error);
                }
            }
            catch (Exception ex)
            {
                return (null, new ErrorDetailDTO { Status = -1, Title = $"{requestUrl}>ErrorMessage {ex.Message}" });
            }

            return (null, null);
        }

        #endregion

        #region Bill Payments

        public async Task<(BillPaymentSuccessResponseDTO?, object)> PostBillPaymentAsync(BillPaymentDTO billPaymentDTO, CancellationToken cancellationToken)
        {
            var auth = await this.GetZamupayIdentityServerAuthTokenAsync(CancellationToken.None);

            if (auth.Item2 != null)
                return (null, auth.Item2);

            var requestUrl = $"{_baseUrlConfiguration.Value.ApiBase}v1/bill-payments";

            try
            {
                var client = new RestClient(requestUrl);
                var request = new RestRequest(requestUrl, Method.Post);

                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", billPaymentDTO.ToJson(),  ParameterType.RequestBody);
                request.AddHeader("Authorization", $"Bearer {auth.Item1?.AccessToken}");
                request.AddHeader("Accept", "text/plain");

                RestResponse response = await client.ExecuteAsync(request);

                var output = response.Content;

                if (response.IsSuccessful && !string.IsNullOrWhiteSpace(output))
                {
                    var zamupayIdentityServerAuthToken = output.FromJson<BillPaymentSuccessResponseDTO>();

                    return (zamupayIdentityServerAuthToken, null);
                }
            }
            catch (Exception ex)
            {
                return (null, new ErrorDetailDTO { Status = -1, Title = $"{requestUrl}>ErrorMessage {ex.Message}" });
            }

            return (null, null);
        }

        public async Task<(BillPaymentResultDTO?, object)> GetBillPaymentAsync(TransactionQueryModelDTO paymentQueryModel, CancellationToken cancellationToken)
        {
            var auth = await this.GetZamupayIdentityServerAuthTokenAsync(CancellationToken.None);

            if (auth.Item2 != null)
                return (null, auth.Item2);

            var requestUrl = $"{_baseUrlConfiguration.Value.ApiBase}v1/bill-payments?{paymentQueryModel.IdType}={paymentQueryModel.Id}";

            try
            {
                var client = new RestClient(requestUrl);
                var request = new RestRequest(requestUrl, Method.Get);

                request.AddHeader("Authorization", $"Bearer {auth.Item1?.AccessToken}");

                RestResponse response = await client.ExecuteAsync(request);

                var output = response.Content;

                if (!string.IsNullOrWhiteSpace(output))
                {
                    if (response.IsSuccessful)
                    {
                        switch (response.StatusCode)
                        {
                            case System.Net.HttpStatusCode.Accepted:
                                break;
                            case System.Net.HttpStatusCode.AlreadyReported:
                                break;
                            case System.Net.HttpStatusCode.Ambiguous:
                                break;
                            case System.Net.HttpStatusCode.BadGateway:
                                break;
                            case System.Net.HttpStatusCode.BadRequest:

                                //var zamupayPaymentOrderQueryErrors = output.FromJson<PaymentFailedWithErrorsResponseDTO>();

                                //return (null, new ErrorDetailDTO
                                //{
                                //    Status = (int)System.Net.HttpStatusCode.BadRequest,
                                //    Title = zamupayPaymentOrderQueryErrors.Message,
                                //    TraceId = zamupayPaymentOrderQueryErrors.Id
                                //});

                                return (null, output.FromJson<PaymentFailedWithErrorsResponseDTO>());

                            //break;
                            case System.Net.HttpStatusCode.Conflict:
                                break;
                            case System.Net.HttpStatusCode.Continue:
                                break;
                            case System.Net.HttpStatusCode.Created:
                                break;
                            case System.Net.HttpStatusCode.EarlyHints:
                                break;
                            case System.Net.HttpStatusCode.ExpectationFailed:
                                break;
                            case System.Net.HttpStatusCode.FailedDependency:
                                break;
                            case System.Net.HttpStatusCode.Forbidden:

                                //var zamupayPaymentOrderNotFound = output.FromJson<PaymentOrderNotFoundDTO>();

                                //return (null, new ErrorDetailDTO
                                //{
                                //    Status = (int)System.Net.HttpStatusCode.NotFound,
                                //    Title = zamupayPaymentOrderNotFound.Message,
                                //    TraceId = zamupayPaymentOrderNotFound.Id
                                //});

                                return (null, output.FromJson<PaymentOrderNotFoundDTO>());

                            //break;
                            case System.Net.HttpStatusCode.Found:
                                break;
                            case System.Net.HttpStatusCode.GatewayTimeout:
                                break;
                            case System.Net.HttpStatusCode.Gone:
                                break;
                            case System.Net.HttpStatusCode.HttpVersionNotSupported:
                                break;
                            case System.Net.HttpStatusCode.IMUsed:
                                break;
                            case System.Net.HttpStatusCode.InsufficientStorage:
                                break;
                            case System.Net.HttpStatusCode.InternalServerError:
                                break;
                            case System.Net.HttpStatusCode.LengthRequired:
                                break;
                            case System.Net.HttpStatusCode.Locked:
                                break;
                            case System.Net.HttpStatusCode.LoopDetected:
                                break;
                            case System.Net.HttpStatusCode.MethodNotAllowed:
                                break;
                            case System.Net.HttpStatusCode.MisdirectedRequest:
                                break;
                            case System.Net.HttpStatusCode.Moved:
                                break;
                            //case System.Net.HttpStatusCode.MovedPermanently:
                            //    break;
                            //case System.Net.HttpStatusCode.MultipleChoices:
                            //    break;
                            case System.Net.HttpStatusCode.MultiStatus:
                                break;
                            case System.Net.HttpStatusCode.NetworkAuthenticationRequired:
                                break;
                            case System.Net.HttpStatusCode.NoContent:
                                break;
                            case System.Net.HttpStatusCode.NonAuthoritativeInformation:
                                break;
                            case System.Net.HttpStatusCode.NotAcceptable:
                                break;
                            case System.Net.HttpStatusCode.NotExtended:
                                break;
                            case System.Net.HttpStatusCode.NotFound:

                                //var zamupayPaymentOrderNotFound = output.FromJson<PaymentOrderNotFoundDTO>();

                                //return (null, new ErrorDetailDTO
                                //{
                                //    Status = (int)System.Net.HttpStatusCode.NotFound,
                                //    Title = zamupayPaymentOrderNotFound.Message,
                                //    TraceId = zamupayPaymentOrderNotFound.Id
                                //});

                                return (null, output.FromJson<PaymentOrderNotFoundDTO>());

                            //break;
                            case System.Net.HttpStatusCode.NotImplemented:
                                break;
                            case System.Net.HttpStatusCode.NotModified:
                                break;
                            case System.Net.HttpStatusCode.OK:

                                return (output.FromJson<BillPaymentResultDTO>(), null);

                            //break;
                            case System.Net.HttpStatusCode.PartialContent:
                                break;
                            case System.Net.HttpStatusCode.PaymentRequired:
                                break;
                            case System.Net.HttpStatusCode.PermanentRedirect:
                                break;
                            case System.Net.HttpStatusCode.PreconditionFailed:
                                break;
                            case System.Net.HttpStatusCode.PreconditionRequired:
                                break;
                            case System.Net.HttpStatusCode.Processing:
                                break;
                            case System.Net.HttpStatusCode.ProxyAuthenticationRequired:
                                break;
                            //case System.Net.HttpStatusCode.Redirect:
                            //    break;
                            case System.Net.HttpStatusCode.RedirectKeepVerb:
                                break;
                            case System.Net.HttpStatusCode.RedirectMethod:
                                break;
                            case System.Net.HttpStatusCode.RequestedRangeNotSatisfiable:
                                break;
                            case System.Net.HttpStatusCode.RequestEntityTooLarge:
                                break;
                            case System.Net.HttpStatusCode.RequestHeaderFieldsTooLarge:
                                break;
                            case System.Net.HttpStatusCode.RequestTimeout:
                                break;
                            case System.Net.HttpStatusCode.RequestUriTooLong:
                                break;
                            case System.Net.HttpStatusCode.ResetContent:
                                break;
                            //case System.Net.HttpStatusCode.SeeOther:
                            //    break;
                            case System.Net.HttpStatusCode.ServiceUnavailable:
                                break;
                            case System.Net.HttpStatusCode.SwitchingProtocols:
                                break;
                            //case System.Net.HttpStatusCode.TemporaryRedirect:
                            //    break;
                            case System.Net.HttpStatusCode.TooManyRequests:
                                break;
                            case System.Net.HttpStatusCode.Unauthorized:

                                //var zamupayPaymentOrderNotFound = output.FromJson<PaymentOrderNotFoundDTO>();

                                //return (null, new ErrorDetailDTO
                                //{
                                //    Status = (int)System.Net.HttpStatusCode.NotFound,
                                //    Title = zamupayPaymentOrderNotFound.Message,
                                //    TraceId = zamupayPaymentOrderNotFound.Id
                                //});

                                return (null, output.FromJson<PaymentOrderNotFoundDTO>());

                            //break;
                            case System.Net.HttpStatusCode.UnavailableForLegalReasons:
                                break;
                            case System.Net.HttpStatusCode.UnprocessableEntity:
                                break;
                            case System.Net.HttpStatusCode.UnsupportedMediaType:
                                break;
                            case System.Net.HttpStatusCode.Unused:
                                break;
                            case System.Net.HttpStatusCode.UpgradeRequired:
                                break;
                            case System.Net.HttpStatusCode.UseProxy:
                                break;
                            case System.Net.HttpStatusCode.VariantAlsoNegotiates:
                                break;
                            default:
                                break;
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(output))
                {
                    var error = output.FromJson<ErrorDetailDTO>();

                    return (null, error);
                }
            }
            catch (Exception ex)
            {
                return (null, new ErrorDetailDTO { Status = -1, Title = $"{requestUrl}>ErrorMessage {ex.Message}" });
            }

            return (null, null);
        }

        #endregion
    }
}