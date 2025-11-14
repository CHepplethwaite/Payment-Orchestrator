using System.Text.Json.Serialization;

namespace TransactionStatus.Responses
{
    public record PaymentTransactionStatusSelfDto
    {
        [JsonPropertyName("href")]
        public string Href { get; init; }
    }
}
