using System;
using System.Text.Json.Serialization;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.Payments.Requests.PaymentMethodDetails
{
    public record VoucherDetailsDto
    {
        [JsonPropertyName("code")]
        public string? Code { get; init; }

        [JsonPropertyName("description")]
        public string? Description { get; init; }

        [JsonPropertyName("value")]
        public string? Value { get; init; }

        [JsonPropertyName("expirationDate")]
        public DateTime? ExpirationDate { get; init; }

        [JsonPropertyName("campaign")]
        public string? Campaign { get; init; }
    }
}
