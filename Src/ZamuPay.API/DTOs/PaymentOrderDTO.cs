using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ZamuPay.API.DTOs
{
    public class PaymentOrderLineDTO
    {
        [JsonPropertyName("remitter")]
        public RemitterDTO Remitter { get; set; } = default!;

        [JsonPropertyName("recipient")]
        public RecipientDTO Recipient { get; set; } = default!;

        [JsonPropertyName("transaction")]
        public TransactionDTO Transaction { get; set; } = default!;
    }

    public class RecipientDTO
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("emailAddress")]
        public string? EmailAddress { get; set; }

        [JsonPropertyName("phoneNumber")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("idType")]
        public string? IdType { get; set; }

        [JsonPropertyName("idNumber")]
        public string? IdNumber { get; set; }

        [JsonPropertyName("financialInstitution")]
        public string? FinancialInstitution { get; set; }

        [JsonPropertyName("institutionIdentifier")]
        public string? InstitutionIdentifier { get; set; }

        [JsonPropertyName("primaryAccountNumber")]
        public string? PrimaryAccountNumber { get; set; }

        [JsonPropertyName("mccmnc")]
        public string? Mccmnc { get; set; }

        [JsonPropertyName("ccy")]
        public int Ccy { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("purpose")]
        public string? Purpose { get; set; }
    }

    public class RemitterDTO
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("phoneNumber")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("idType")]
        public string? IdType { get; set; }

        [JsonPropertyName("idNumber")]
        public string? IdNumber { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("ccy")]
        public int Ccy { get; set; }

        [JsonPropertyName("financialInstitution")]
        public string? FinancialInstitution { get; set; }

        [JsonPropertyName("sourceOfFunds")]
        public string? SourceOfFunds { get; set; }

        [JsonPropertyName("principalActivity")]
        public string? PrincipalActivity { get; set; }
    }

    public class PaymentOrderDTO
    {
        [JsonPropertyName("originatorConversationId")]
        public string? OriginatorConversationId { get; set; }

        [JsonPropertyName("paymentNotes")]
        public string? PaymentNotes { get; set; }

        [JsonPropertyName("paymentOrderLines")]
        public List<PaymentOrderLineDTO> PaymentOrderLines { get; set; } = default!;
    }

    public class TransactionDTO
    {
        [JsonPropertyName("routeId")]
        public string? RouteId { get; set; }

        [JsonPropertyName("ChannelType")]
        public int ChannelType { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("reference")]
        public string? Reference { get; set; }

        [JsonPropertyName("systemTraceAuditNumber")]
        public string? SystemTraceAuditNumber { get; set; }
    }


}
