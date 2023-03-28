using System.Text.Json.Serialization;
using System;

namespace ZamuPay.API.DTOs
{
    public class PaymentSuccessResponseDTO
    {
        [JsonPropertyName("message")]
        public PaymentSuccessMessage? Message { get; set; }
    }

    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    public class PaymentSuccessMessage
    {
        [JsonPropertyName("appDomainName")]
        public string? AppDomainName { get; set; }

        [JsonPropertyName("systemConversationId")]
        public string? SystemConversationId { get; set; }

        [JsonPropertyName("originatorConversationId")]
        public string? OriginatorConversationId { get; set; }

        [JsonPropertyName("remarks")]
        public string? Remarks { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
    }



}
