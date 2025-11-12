using System.Text.Json.Serialization;

public record FeeCheckRequestDto
{
    [JsonPropertyName("correlatorId")]
    public string CorrelatorId { get; init; }

    [JsonPropertyName("paymentDate")]
    public DateTime PaymentDate { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("callingSystem")]
    public string CallingSystem { get; init; }

    [JsonPropertyName("transactionType")]
    public string TransactionType { get; init; }

    [JsonPropertyName("targetSystem")]
    public string TargetSystem { get; init; }

    [JsonPropertyName("callbackUrl")]
    public string CallbackUrl { get; init; }

    [JsonPropertyName("quoteId")]
    public string QuoteId { get; init; }

    [JsonPropertyName("channel")]
    public string Channel { get; init; }

    [JsonPropertyName("description")]
    public string Description { get; init; }

    [JsonPropertyName("authorizationCode")]
    public string AuthorizationCode { get; init; }

    [JsonPropertyName("feeBearer")]
    public string FeeBearer { get; init; }

    [JsonPropertyName("amount")]
    public MonetaryDto Amount { get; init; }

    [JsonPropertyName("taxAmount")]
    public MonetaryDto TaxAmount { get; init; }

    [JsonPropertyName("totalAmount")]
    public MonetaryDto TotalAmount { get; init; }

    [JsonPropertyName("payer")]
    public PayerDto Payer { get; init; }

    [JsonPropertyName("payees")]
    public List<PayeeDto> Payees { get; init; }

    [JsonPropertyName("paymentMethod")]
    public PaymentMethodDto PaymentMethod { get; init; }

    [JsonPropertyName("additionalInformation")]
    public List<AdditionalInformationDto> AdditionalInformation { get; init; }

    [JsonPropertyName("segment")]
    public string Segment { get; init; }

    [JsonPropertyName("includePayerCharges")]
    public bool IncludePayerCharges { get; init; }
}
