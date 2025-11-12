using System;
using System.Text.Json.Serialization;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.SMS.Inbound
{
    public record SmsInboundMessagesQueryResponseDto
    {
        [JsonPropertyName("statusCode")]
        public string StatusCode { get; init; }

        [JsonPropertyName("statusMessage")]
        public string StatusMessage { get; init; }

        [JsonPropertyName("transactionId")]
        public string TransactionId { get; init; }

        [JsonPropertyName("data")]
        public SmsInboundMessagesQueryDataDto Data { get; init; }
    }
}
