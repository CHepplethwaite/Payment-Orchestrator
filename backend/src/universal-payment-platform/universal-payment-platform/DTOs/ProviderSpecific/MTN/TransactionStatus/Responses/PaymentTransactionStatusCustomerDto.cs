using System.Text.Json.Serialization;

namespace TransactionStatus.Responses
{
    public record PaymentTransactionStatusCustomerDto
    {
        [JsonPropertyName("firstname")]
        public string? Firstname { get; init; }

        [JsonPropertyName("surname")]
        public string? Surname { get; init; }

        [JsonPropertyName("email")]
        public string? Email { get; init; }

        [JsonPropertyName("msisdn")]
        public string? Msisdn { get; init; }
    }
}
