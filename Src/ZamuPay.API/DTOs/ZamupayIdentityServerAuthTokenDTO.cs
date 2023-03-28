using System.Text.Json.Serialization;

namespace ZamuPay.API.DTOs
{
    public class ZamupayIdentityServerAuthTokenDTO
    {
        [property: JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = default!;

        [property: JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; } = default!;

        [property: JsonPropertyName("token_type")]
        public string TokenType { get; set; } = default!;

        [property: JsonPropertyName("scope")]
        public string Scope { get; set; } = default!;
    }
}