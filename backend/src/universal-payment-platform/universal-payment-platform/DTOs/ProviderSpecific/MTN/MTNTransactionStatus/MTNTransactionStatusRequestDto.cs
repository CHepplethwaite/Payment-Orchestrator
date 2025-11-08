using System.Text.Json.Serialization;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN
{
    public record MTNTransactionStatusRequestDto
    {
        // Path Parameter
        [JsonIgnore]
        public required string CorrelatorId { get; init; }

        // Query Parameters
        [JsonPropertyName("amount")]
        public decimal? Amount { get; init; }

        [JsonPropertyName("customerId")]
        public string? CustomerId { get; init; }

        [JsonPropertyName("description")]
        public string? Description { get; init; }

        [JsonPropertyName("paymentType")]
        public string? PaymentType { get; init; } // e.g., Airtime

        [JsonPropertyName("targetSystem")]
        public string? TargetSystem { get; init; } // e.g., EWP, ECW, CELD

        // Headers (optional properties can be set in HTTP request)
        [JsonIgnore]
        public string? XAuthorization { get; init; }

        [JsonIgnore]
        public string? TransactionId { get; init; }
    }
}
