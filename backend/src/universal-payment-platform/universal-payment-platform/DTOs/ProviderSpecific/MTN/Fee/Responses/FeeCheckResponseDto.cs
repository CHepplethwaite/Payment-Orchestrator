using universal_payment_platform.DTOs.ProviderSpecific.MTN.Fee.Responses;
using System.Text.Json.Serialization;

public record FeeCheckResponseDto
{
    [JsonPropertyName("statusCode")]
    public string StatusCode { get; init; }

    [JsonPropertyName("error")]
    public string Error { get; init; }

    [JsonPropertyName("sequenceNo")]
    public string SequenceNo { get; init; }

    [JsonPropertyName("data")]
    public FeeCheckDataDto Data { get; init; }
}
