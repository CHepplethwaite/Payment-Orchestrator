using System.Text.Json.Serialization;
using universal_payment_platform.DTOs.ProviderSpecific.MTN.TransactionStatus.Responses;

namespace TransactionStatus.Responses
{
    public record PaymentTransactionStatusLinksDto
    {
        [JsonPropertyName("self")]
        public PaymentTransactionStatusSelfDto? Self { get; init; }
    }
}
