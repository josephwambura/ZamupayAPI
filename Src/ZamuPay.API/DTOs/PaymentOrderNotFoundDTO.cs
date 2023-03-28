using System.Text.Json.Serialization;

namespace ZamuPay.API.DTOs
{
    public class PaymentOrderNotFoundDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;

        [JsonPropertyName("idType")]
        public string IdType { get; set; } = default!;

        [JsonPropertyName("message")]
        public string Message { get; set; } = default!;
    }


}
