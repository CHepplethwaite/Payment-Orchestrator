using System.Text.Json.Serialization;

public record PaymentAgreementEligibilityRequestDto
{
    [JsonPropertyName("billingAccountNumber")]
    public string? BillingAccountNumber { get; init; }
}
