using System.Text.Json.Serialization;

namespace Application.DTOs.Payments.Responses
{
    public record PaymentItemDto
    {
        [JsonPropertyName("totalAmount")]
        public MoneyDto? TotalAmount { get; init; }

        [JsonPropertyName("type")]
        public string? Type { get; init; }
    }
}
