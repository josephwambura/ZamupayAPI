using System.Text.Json.Serialization;

namespace ZamuPay.API.DTOs
{
    public class ErrorDetailDTO
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("traceId")]
        public string? TraceId { get; set; }
    }
}