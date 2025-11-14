using System.Text.Json.Serialization;

namespace universal_payment_platform.DTOs.ProviderSpecific.MTN.TransactionStatus.Responses
{
    public record PaymentTransactionStatusSelfDto
    {
        [JsonPropertyName("href")]
        public string? Href { get; init; }
    }
}
