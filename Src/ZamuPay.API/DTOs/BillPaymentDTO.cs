using System.Text.Json.Serialization;

namespace ZamuPay.API.DTOs
{
    public class BillPaymentDTO
    {
        [JsonPropertyName("accountNumber")]
        public string AccountNumber { get; set; }

        [JsonPropertyName("serviceCode")]
        public string ServiceCode { get; set; }

        [JsonPropertyName("transactionType")]
        public int TransactionType { get; set; }

        [JsonPropertyName("countryCode")]
        public string CountryCode { get; set; }

        [JsonPropertyName("msisdn")]
        public string Msisdn { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("customerNames")]
        public string CustomerNames { get; set; }

        [JsonPropertyName("currencyCode")]
        public string CurrencyCode { get; set; }

        [JsonPropertyName("saveBillerNumber")]
        public bool SaveBillerNumber { get; set; }

        [JsonPropertyName("isExpressPay")]
        public bool IsExpressPay { get; set; }

        [JsonPropertyName("paymentMethod")]
        public int PaymentMethod { get; set; }

        [JsonPropertyName("narration")]
        public string Narration { get; set; }

        [JsonPropertyName("callbackURL")]
        public string CallbackURL { get; set; }

        [JsonPropertyName("originatorConversationId")]
        public string OriginatorConversationId { get; set; }

        [JsonPropertyName("shortCode")]
        public string ShortCode { get; set; }

        [JsonPropertyName("byPassCustomerValidation")]
        public bool ByPassCustomerValidation { get; set; }
    }
}
