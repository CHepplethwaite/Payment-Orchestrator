using System.Text.Json.Serialization;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.Payments.Requests.PaymentMethodDetails
{
    public record BankCardDetailsDto
    {
        [JsonPropertyName("brand")]
        public string? Brand { get; init; }

        [JsonPropertyName("type")]
        public string? Type { get; init; }

        [JsonPropertyName("cardNumber")]
        public string? CardNumber { get; init; }

        [JsonPropertyName("expirationDate")]
        public DateTime? ExpirationDate { get; init; }

        [JsonPropertyName("cvv")]
        public string? CVV { get; init; }

        [JsonPropertyName("lastFourDigits")]
        public string? LastFourDigits { get; init; }

        [JsonPropertyName("nameOnCard")]
        public string? NameOnCard { get; init; }

        [JsonPropertyName("bank")]
        public string? Bank { get; init; }

        [JsonPropertyName("pin")]
        public string? Pin { get; init; }
    }
}
