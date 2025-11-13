using System.Text.Json.Serialization;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.Payments.Requests.PaymentMethodDetails
{
    public record TokenizedCardDetailsDto
    {
        [JsonPropertyName("brand")]
        public string? Brand { get; init; }

        [JsonPropertyName("type")]
        public string? Type { get; init; }

        [JsonPropertyName("lastFourDigits")]
        public string? LastFourDigits { get; init; }

        [JsonPropertyName("tokenType")]
        public string? TokenType { get; init; }

        [JsonPropertyName("token")]
        public string? Token { get; init; }

        [JsonPropertyName("issuer")]
        public string? Issuer { get; init; }
    }
}
