public record SmsOutboundRequestDto(
    string SenderAddress,                  // MSISDN or virtual MSISDN of sender
    IReadOnlyList<string> ReceiverAddress, // Array of recipient MSISDN(s)
    string Message,                        // SMS message (max 160 characters)
    string ClientCorrelator                // Optional unique request identifier
);
