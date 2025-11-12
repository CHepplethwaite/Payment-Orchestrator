using System.Text.Json.Serialization;

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
        public MoneyDto? Amount { get; init; }

        [JsonPropertyName("status")]
        public string? Status { get; init; }
    }
}
