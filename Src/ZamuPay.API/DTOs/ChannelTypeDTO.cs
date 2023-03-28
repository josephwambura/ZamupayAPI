using System.Text.Json.Serialization;

namespace ZamuPay.API.DTOs
{
    public class ChannelTypeDTO
    {
        [JsonPropertyName("channelType")]
        public int ChannelType { get; set; }

        [JsonPropertyName("channelDescription")]
        public string ChannelDescription { get; set; } = default!;
    }
}


