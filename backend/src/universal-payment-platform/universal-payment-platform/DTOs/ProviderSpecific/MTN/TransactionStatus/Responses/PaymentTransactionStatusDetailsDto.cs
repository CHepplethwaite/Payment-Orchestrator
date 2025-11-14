using System.Text.Json.Serialization;

namespace TransactionStatus.Responses
{
    public record PaymentTransactionStatusDetailsDto
    {
        [JsonPropertyName("brand")]
        public string? Brand { get; init; }

        [JsonPropertyName("fulfillmentMsisdn")]
        public string? FulfillmentMsisdn { get; init; }

        [JsonPropertyName("issuer")]
        public string? Issuer { get; init; }
    }
}
