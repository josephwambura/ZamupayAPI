using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ZamuPay.API.DTOs
{
    public class PaymentOrderQueryDTO
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("idType")]
        public string? IdType { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }


}
