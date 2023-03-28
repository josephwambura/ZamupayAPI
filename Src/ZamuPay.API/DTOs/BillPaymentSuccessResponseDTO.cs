using System.Text.Json.Serialization;
using System;

namespace ZamuPay.API.DTOs
{
    public class BillPaymentSuccessResponseDTO
    {
        [JsonPropertyName("message")]
        public BillPaymentSuccessMessage? Message { get; set; }
    }

    public class BillPaymentSuccessMessage
    {
        [JsonPropertyName("appDomainName")]
        public string AppDomainName { get; set; }

        [JsonPropertyName("remarks")]
        public string Remarks { get; set; }

        [JsonPropertyName("originatorConversationId")]
        public string OriginatorConversationId { get; set; }

        [JsonPropertyName("systemConversationId")]
        public string SystemConversationId { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("dueDate")]
        public string DueDate { get; set; }

        [JsonPropertyName("dueAmount")]
        public double DueAmount { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("accountCustomerName")]
        public string AccountCustomerName { get; set; }

        [JsonPropertyName("active")]
        public string Active { get; set; }

        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("statusDescription")]
        public string StatusDescription { get; set; }
    }



}
