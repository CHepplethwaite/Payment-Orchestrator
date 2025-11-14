using System.Text.Json.Serialization;

public record PromiseToPayEligibilityDetailsDto
{
    [JsonPropertyName("eligibilityStatus")]
    public string? EligiblityStatus { get; init; }

    [JsonPropertyName("accountBalance")]
    public string? AccountBalance { get; init; }

    [JsonPropertyName("minimumAmount")]
    public DateTime? MinimumAmount { get; init; }

    [JsonPropertyName("paymentStartDate")]
    public DateTime? PaymentStartDate { get; init; }
}
