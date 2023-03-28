using ZamuPay.API.DTOs;

namespace ZamuPay.API.Extensions
{
    public static class TransactionExtensions
    {
        public static BillPaymentDTO CreateBillPayment(string accountName, string serviceCode, byte transactionType,
            string countryCode, string msisdn, decimal amount, string customerNames, string currencyCode, bool saveBillerNumber,
            byte paymentMethod, string narration, string callbackUrl, string originatorConversationId, string shortCode, bool byPassCustomerValidation)
        {
            var billPaymentDTO = new BillPaymentDTO
            {
                AccountNumber = accountName,
                ServiceCode = serviceCode,
                TransactionType = transactionType,
                CountryCode = countryCode,
                Msisdn = msisdn,
                Amount = amount,
                CustomerNames = customerNames,
                CurrencyCode = currencyCode,
                SaveBillerNumber = saveBillerNumber,
                PaymentMethod = paymentMethod,
                Narration = narration,
                CallbackURL = callbackUrl,
                OriginatorConversationId = originatorConversationId,
                ShortCode = shortCode,
                ByPassCustomerValidation = byPassCustomerValidation
            };

            return billPaymentDTO;
        }
    }
}
