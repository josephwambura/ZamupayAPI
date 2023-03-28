using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using ZamuPay.API.DTOs;

namespace ZamuPay.API.Services
{
    public interface IZamupayService
    {
        #region Identity Server

        public Task<(ZamupayIdentityServerAuthTokenDTO?, ErrorDetailDTO)> GetZamupayIdentityServerAuthTokenAsync(CancellationToken cancellationToken);

        #endregion

        #region Routes

        public Task<(ZamupayRoutesDTO?, ErrorDetailDTO)> GetZamupayRoutesAsync(int expirationTime, CancellationToken cancellationToken);

        public Task<(RouteDTO?, ErrorDetailDTO)> GetZamupayRouteAsync(Guid id, int expirationTime, CancellationToken cancellationToken);

        public Task<(IEnumerable<RouteDTO>?, ErrorDetailDTO)> GetZamupayRoutesByCategoryAsync(string category, int expirationTime, CancellationToken cancellationToken);

        public Task<(IEnumerable<ChannelTypeDTO>?, ErrorDetailDTO)> GetZamupayRouteChannelTypesAsync(Guid id, int expirationTime, CancellationToken cancellationToken);

        #endregion

        #region Payment Orders

        public Task<(PaymentOrderDTO?, object)> GetPaymentOrderAsync(TransactionQueryModelDTO paymentQueryModel, CancellationToken cancellationToken);

        #endregion

        #region Airtime Purchases

        public Task<(AirtimePurchaseRequestDTO?, object)> PostAirtimePurchaseRequestAsync(AirtimePurchaseRequestDTO paymentQueryModel, CancellationToken cancellationToken);

        public Task<(PaymentOrderDTO?, object)> GetAirtimePurchaseAsync(TransactionQueryModelDTO paymentQueryModel, CancellationToken cancellationToken);

        #endregion

        #region Bill Payments

        public Task<(BillPaymentSuccessResponseDTO?, object)> PostBillPaymentAsync(BillPaymentDTO billPaymentDTO, CancellationToken cancellationToken);

        public Task<(BillPaymentResultDTO?, object)> GetBillPaymentAsync(TransactionQueryModelDTO paymentQueryModel, CancellationToken cancellationToken);

        #endregion
    }
}