using System.Text.Json.Serialization;
using universal_payment_platform.DTOs.ProviderSpecific.MTN.PaymentAgreements.Eligibility.Responses;

public record PromiseToPayEligibilityDataDto
{
    [JsonPropertyName("promiseToPayEligibilityDetails")]
    public PromiseToPayEligibilityDetailsDto? PromiseToPayEligibilityDetails { get; init; }
}
