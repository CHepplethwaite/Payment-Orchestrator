using System.Text.Json.Serialization;

public record PaymentAgreementRequestDto
{
    [JsonPropertyName("billingAccountNo")]
    public string BillingAccountNo { get; init; }

    [JsonPropertyName("serviceName")]
    public string ServiceName { get; init; }

    [JsonPropertyName("promiseOpenDate")]
    public DateTime PromiseOpenDate { get; init; }

    [JsonPropertyName("promiseAmount")]
    public double PromiseAmount { get; init; }

    [JsonPropertyName("numberOfInstallments")]
    public string NumberOfInstallments { get; init; }

    [JsonPropertyName("durationUOM")]
    public string DurationUOM { get; init; }

    [JsonPropertyName("promiseThreshold")]
    public string PromiseThreshold { get; init; }
}