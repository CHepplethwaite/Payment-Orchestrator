using System.Text.Json.Serialization;
using universal_payment_platform.DTOs.ProviderSpecific.MTN.Shared;

namespace Application.DTOs.Payments.Responses
{
    public record PaymentRecordDto
    {
        [JsonPropertyName("recordId")]
        public string? RecordId { get; init; }

        [JsonPropertyName("description")]
        public string? Description { get; init; }

        [JsonPropertyName("recordDate")]
        public DateTime? RecordDate { get; init; }

        [JsonPropertyName("amount")]
        public MonetaryTypeDto? Amount { get; init; }

        [JsonPropertyName("status")]
        public string? Status { get; init; }
    }
}
