using System.Text.Json.Serialization;
using universal_payment_platform.DTOs.ProviderSpecific.MTN.PaymentAgreements.Eligibility.Responses;

public record PaymentAgreementEligibilityResponseDto
{
    [JsonPropertyName("statusCode")]
    public string StatusCode { get; init; }

    [JsonPropertyName("statusMessage")]
    public string StatusMessage { get; init; }

    [JsonPropertyName("transactionId")]
    public string TransactionId { get; init; }

    [JsonPropertyName("data")]
    public PaymentAgreementEligibilityDataDto Data { get; init; }
}
