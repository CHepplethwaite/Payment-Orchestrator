using System.Text.Json.Serialization;
using universal_payment_platform.DTOs.ProviderSpecific.MTN.PaymentAgreements.Eligibility.Responses;

public record PaymentAgreementEligibilityDataDto
{
    [JsonPropertyName("promiseToPayEligibilityDetails")]
    public PromiseToPayEligibilityDetailsDto PromiseToPayEligibilityDetails { get; init; }
}
