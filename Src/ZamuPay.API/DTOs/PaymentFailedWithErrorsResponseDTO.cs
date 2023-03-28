using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;

namespace ZamuPay.API.DTOs
{
    public class PaymentFailedWithErrorsResponseDTO
    {
        [JsonPropertyName("appDomainName")]
        public string? AppDomainName { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("systemConversationId")]
        public string? SystemConversationId { get; set; }

        [JsonPropertyName("originatorConversationId")]
        public string? OriginatorConversationId { get; set; }

        [JsonPropertyName("errors")]
        public List<Error>? Errors { get; set; }
    }

    public class Error
    {
        [JsonPropertyName("field")]
        public string Field { get; set; } = default!;

        [JsonPropertyName("message")]
        public string Message { get; set; } = default!;
    }


}
