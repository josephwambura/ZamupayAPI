using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ZamuPay.API.DTOs
{
    public class Recipient
    {
        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; } = default!;

        [JsonPropertyName("currencyCode")]
        public string CurrencyCode { get; set; } = default!;

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;
    }

    public class AirtimePurchaseRequestDTO
    {
        [JsonPropertyName("originatorConversationId")]
        public string OriginatorConversationId { get; set; } = default!;

        [JsonPropertyName("callbackURL")]
        public string CallbackURL { get; set; } = default!;

        [JsonPropertyName("recipients")]
        public List<Recipient> Recipients { get; set; } = default!;
    }
}