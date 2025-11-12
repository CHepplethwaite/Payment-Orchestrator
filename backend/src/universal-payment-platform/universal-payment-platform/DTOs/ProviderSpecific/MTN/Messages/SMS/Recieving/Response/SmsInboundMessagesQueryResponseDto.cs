using System;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.SMS.Inbound
{
    public record SmsInboundMessagesQueryResponseDto(
        string StatusCode,          // Canonical error code (e.g. 0000)
        string StatusMessage,       // Success or failure message
        string TransactionId,       // MADAPI transaction ID
        SmsInboundMessagesQueryDataDto Data  // Message batch details
    );
}
