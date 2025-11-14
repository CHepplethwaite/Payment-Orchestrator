using System.Text.Json.Serialization;
using UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.Payments.Enums;
using UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.Payments.Requests.PaymentMethodDetails;

namespace universal_payment_platform.DTOs.ProviderSpecific.MTN.Shared
{
    public record PaymentMethodDto
    {
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("description")]
        public string? Description { get; init; }

        [JsonPropertyName("validFrom")]
        public DateTime? ValidFrom { get; init; }

        [JsonPropertyName("validTo")]
        public DateTime? ValidTo { get; init; }

        [JsonPropertyName("type")]
        public required PaymentMethodType Type { get; init; }

        [JsonPropertyName("details")]
        public PaymentMethodDetailsDto? Details { get; init; }
    }
}
