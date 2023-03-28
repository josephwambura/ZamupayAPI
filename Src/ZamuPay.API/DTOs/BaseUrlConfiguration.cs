namespace ZamuPay.API.DTOs
{
    public class BaseUrlConfiguration
    {
        public const string CONFIG_NAME = "baseUrls";

        public string? ApiBase { get; set; }
        public string? IdentityServerBase { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? GrantType { get; set; }
        public string? Scope { get; set; }
        public bool SandBoxTesing { get; set; }
    }
}

