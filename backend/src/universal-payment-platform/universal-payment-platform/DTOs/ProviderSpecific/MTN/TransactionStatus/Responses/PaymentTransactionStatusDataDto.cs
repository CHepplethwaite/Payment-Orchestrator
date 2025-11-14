using System.Text.Json.Serialization;

namespace TransactionStatus.Responses
{
    public record PaymentTransactionStatusDataDto
    {
        [JsonPropertyName("financialTransactionId")]
        public string? FinancialTransactionId { get; init; }

        [JsonPropertyName("status")]
        public string? Status { get; init; }

        [JsonPropertyName("requestType")]
        public string? RequestType { get; init; }

        [JsonPropertyName("fulfillmentStatus")]
        public string? FulfillmentStatus { get; init; }

        [JsonPropertyName("transactionRefParent")]
        public string? TransactionRefParent { get; init; }

        [JsonPropertyName("transactionDescription")]
        public string? TransactionDescription { get; init; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; init; }

        [JsonPropertyName("date")]
        public string? Date { get; init; }

        [JsonPropertyName("channel")]
        public string? Channel { get; init; }

        [JsonPropertyName("product")]
        public string? Product { get; init; }

        [JsonPropertyName("description")]
        public string? Description { get; init; }

        [JsonPropertyName("customer")]
        public PaymentTransactionStatusCustomerDto? Customer { get; init; }

        [JsonPropertyName("charges")]
        public PaymentTransactionStatusChargesDto? Charges { get; init; }
    }
}
