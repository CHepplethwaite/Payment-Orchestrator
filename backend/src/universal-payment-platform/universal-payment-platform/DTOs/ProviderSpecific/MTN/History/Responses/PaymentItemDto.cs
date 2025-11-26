using universal_payment_platform.DTOs.ProviderSpecific.MTN.Shared;

using System.Text.Json.Serialization;

namespace Application.DTOs.Payments.Responses
{
    public record PaymentItemDto
    {
        [JsonPropertyName("totalAmount")]
        public MonetaryTypeDto? TotalAmount { get; init; }

        [JsonPropertyName("type")]
        public string? Type { get; init; }
    }
}
