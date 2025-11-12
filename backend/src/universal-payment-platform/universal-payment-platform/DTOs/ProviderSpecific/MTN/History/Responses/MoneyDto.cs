using System.Text.Json.Serialization;

namespace Application.DTOs.Payments.Responses
{
    public record MoneyDto
    {
        [JsonPropertyName("amount")]
        public decimal Amount { get; init; }

        [JsonPropertyName("units")]
        public string Units { get; init; } = "";
    }
}
