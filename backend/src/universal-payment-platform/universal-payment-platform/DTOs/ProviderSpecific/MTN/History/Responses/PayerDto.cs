using System.Text.Json.Serialization;

namespace Application.DTOs.Payments.Responses
{
    public record PayerDto
    {
        [JsonPropertyName("payerIdType")]
        public string? PayerIdType { get; init; }

        [JsonPropertyName("payerId")]
        public string? PayerId { get; init; }

        [JsonPropertyName("payerName")]
        public string? PayerName { get; init; }

        [JsonPropertyName("payerSurname")]
        public string? PayerSurname { get; init; }

        [JsonPropertyName("payerEmail")]
        public string? PayerEmail { get; init; }
    }
}
