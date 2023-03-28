using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ZamuPay.API.DTOs
{
    public class RouteDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;

        [JsonPropertyName("category")]
        public string Category { get; set; } = default!;

        [JsonPropertyName("categoryDescription")]
        public string CategoryDescription { get; set; } = default!;

        [JsonPropertyName("transactionTypeId")]
        public string TransactionTypeId { get; set; } = default!;

        [JsonPropertyName("categoryIsEnabled")]
        public bool CategoryIsEnabled { get; set; } = default!;

        [JsonPropertyName("routeIntergration")]
        public string RouteIntergration { get; set; } = default!;

        [JsonPropertyName("country")]
        public string Country { get; set; } = default!;

        [JsonPropertyName("routeIsActive")]
        public bool RouteIsActive { get; set; } = default!;

        [JsonPropertyName("channelTypes")]
        public IReadOnlyList<ChannelTypeDTO> ChannelTypes { get; set; } = default!;
    }
}

