using System.Text.Json.Serialization;

namespace Application.DTOs.Payments.Responses
{
    public record RelatedPartyDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        [JsonPropertyName("role")]
        public string? Role { get; init; }

        [JsonPropertyName("name")]
        public string? Name { get; init; }
    }
}
