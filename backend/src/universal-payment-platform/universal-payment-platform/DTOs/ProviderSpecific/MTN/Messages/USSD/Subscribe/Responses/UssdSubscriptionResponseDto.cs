using System.Text.Json.Serialization;
using universal_payment_platform.DTOs.ProviderSpecific.MTN.Messages.USSD.Responses;

public record UssdSubscriptionResponseDto
{
    [JsonPropertyName("statusCode")]
    public string StatusCode { get; init; }

    [JsonPropertyName("statusMessage")]
    public string StatusMessage { get; init; }

    [JsonPropertyName("transactionId")]
    public string TransactionId { get; init; }

    [JsonPropertyName("data")]
    public UssdSubscriptionDataDto Data { get; init; }

    [JsonPropertyName("links")]
    public LinkDto Links { get; init; }
}
