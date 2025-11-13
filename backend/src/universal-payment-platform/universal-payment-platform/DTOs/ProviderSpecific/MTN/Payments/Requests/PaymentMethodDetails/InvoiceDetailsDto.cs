using System;
using System.Text.Json.Serialization;
using UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.Payments.Enums;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.Payments.Requests.PaymentMethodDetails
{
    public record InvoiceDetailsDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        [JsonPropertyName("type")]
        public string? Type { get; init; }

        [JsonPropertyName("frequency")]
        public InvoiceFrequencyType? Frequency { get; init; }

        [JsonPropertyName("startDate")]
        public DateTime? StartDate { get; init; }

        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; init; }

        [JsonPropertyName("retryOnFail")]
        public bool? RetryOnFail { get; init; }

        [JsonPropertyName("deactivateOnFail")]
        public bool? DeactivateOnFail { get; init; }

        [JsonPropertyName("callbackUrl")]
        public string? CallbackUrl { get; init; }

        [JsonPropertyName("retryRun")]
        public string? RetryRun { get; init; }

        [JsonPropertyName("retryFrequency")]
        public string? RetryFrequency { get; init; }
    }
}
