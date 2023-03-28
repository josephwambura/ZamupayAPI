using System.Text.Json.Serialization;
using System;

namespace ZamuPay.API.DTOs
{
    public class PaymentFailedResponseDTO
    {
        [JsonPropertyName("appDomainName")]
        public string? AppDomainName { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("systemConversationId")]
        public string? SystemConversationId { get; set; }

        [JsonPropertyName("originatorConversationId")]
        public string? OriginatorConversationId { get; set; }

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }
    }


}
