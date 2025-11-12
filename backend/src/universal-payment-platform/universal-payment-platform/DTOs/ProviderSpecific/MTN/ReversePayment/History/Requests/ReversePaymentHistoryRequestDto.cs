using System.Text.Json.Serialization;

public record ReversePaymentHistoryRequestDto
{
    [JsonPropertyName("transactionType")]
    public string TransactionType { get; init; }

    [JsonPropertyName("amount")]
    public decimal? Amount { get; init; }

    [JsonPropertyName("startDate")]
    public DateTime? StartDate { get; init; }

    [JsonPropertyName("endDate")]
    public DateTime? EndDate { get; init; }

    [JsonPropertyName("limit")]
    public int? Limit { get; init; } = 50;

    [JsonPropertyName("pageNo")]
    public int? PageNo { get; init; }

    [JsonPropertyName("nodeId")]
    public string NodeId { get; init; }

    [JsonPropertyName("otherFri")]
    public string OtherFri { get; init; }

    [JsonPropertyName("posMsisdn")]
    public string PosMsisdn { get; init; }

    [JsonPropertyName("quoteId")]
    public string QuoteId { get; init; }

    [JsonPropertyName("transactionStatus")]
    public string TransactionStatus { get; init; }
}