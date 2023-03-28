using System.Text.Json.Serialization;

namespace ZamuPay.API.DTOs
{
    public class BillPaymentResultDTO
    {
        [JsonPropertyName("systemConversationId")]
        public string SystemConversationId { get; set; }

        [JsonPropertyName("originatorConversationId")]
        public string OriginatorConversationId { get; set; }

        [JsonPropertyName("serviceCode")]
        public string ServiceCode { get; set; }

        [JsonPropertyName("accountNumber")]
        public string AccountNumber { get; set; }

        [JsonPropertyName("msisdn")]
        public string Msisdn { get; set; }

        [JsonPropertyName("amount")]
        public double Amount { get; set; }

        [JsonPropertyName("datePaymentReceived")]
        public string DatePaymentReceived { get; set; }

        [JsonPropertyName("accountCustomerName")]
        public string AccountCustomerName { get; set; }

        [JsonPropertyName("customerNames")]
        public string CustomerNames { get; set; }

        [JsonPropertyName("countryCode")]
        public string CountryCode { get; set; }

        [JsonPropertyName("currencyCode")]
        public string CurrencyCode { get; set; }

        [JsonPropertyName("transactionType")]
        public int TransactionType { get; set; }

        [JsonPropertyName("dueAmount")]
        public double DueAmount { get; set; }

        [JsonPropertyName("dueDate")]
        public object DueDate { get; set; }

        [JsonPropertyName("narration")]
        public string Narration { get; set; }

        [JsonPropertyName("receiptNumber")]
        public object ReceiptNumber { get; set; }

        [JsonPropertyName("receiverNarration")]
        public object ReceiverNarration { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("transactionStatusDescription")]
        public object TransactionStatusDescription { get; set; }

        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("statusDescription")]
        public string StatusDescription { get; set; }

        [JsonPropertyName("totalRecordsPendingAck")]
        public string TotalRecordsPendingAck { get; set; }

        [JsonPropertyName("totalRecordsPendingQuery")]
        public string TotalRecordsPendingQuery { get; set; }

        [JsonPropertyName("active")]
        public object Active { get; set; }

        [JsonPropertyName("callbackURL")]
        public string CallbackURL { get; set; }

        [JsonPropertyName("byPassCustomerValidation")]
        public bool ByPassCustomerValidation { get; set; }
    }
}
