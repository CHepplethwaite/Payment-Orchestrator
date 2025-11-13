using System.Text.Json.Serialization;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.Payments.Requests.PaymentMethodDetails
{
    public record DigitalWalletDetailsDto
    {
        [JsonPropertyName("service")]
        public string? Service { get; init; }

        [JsonPropertyName("walletId")]
        public string? WalletId { get; init; }

        [JsonPropertyName("walletUri")]
        public string? WalletUri { get; init; }
    }
}
