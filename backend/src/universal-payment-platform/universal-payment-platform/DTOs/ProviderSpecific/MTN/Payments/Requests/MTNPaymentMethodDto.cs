using System.Text.Json.Serialization;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.Payments.Requests
{
    public record MTNPaymentMethodDto
    {
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("description")]
        public string? Description { get; init; }

        [JsonPropertyName("validFrom")]
        public string? ValidFrom { get; init; }

        [JsonPropertyName("validTo")]
        public string? ValidTo { get; init; }

        [JsonPropertyName("type")]
        public required string Type { get; init; }

        [JsonPropertyName("details")]
        public object? Details { get; init; }

        [JsonPropertyName("status")]
        public string? Status { get; init; }

        [JsonPropertyName("statusDate")]
        public string? StatusDate { get; init; }
    }
}
