using System.Text.Json.Serialization;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.Payments.Requests.PaymentMethodDetails
{
    public record BankAccountDebitDetailsDto
    {
        [JsonPropertyName("accountNumber")]
        public string? AccountNumber { get; init; }

        [JsonPropertyName("accountNumberType")]
        public string? AccountNumberType { get; init; }

        [JsonPropertyName("BIC")]
        public string? BIC { get; init; }

        [JsonPropertyName("owner")]
        public string? Owner { get; init; }

        [JsonPropertyName("bank")]
        public string? Bank { get; init; }
    }
}
