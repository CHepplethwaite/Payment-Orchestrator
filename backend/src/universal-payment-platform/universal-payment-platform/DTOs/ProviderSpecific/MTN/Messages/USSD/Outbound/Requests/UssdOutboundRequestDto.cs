public record UssdOutboundRequestDto(
    string SessionId,      // Unique identifier of the session
    string MessageType,    // 0-Begin|1-Continue|2-End|3-Notification|4-Cancel|5-Timeout
    string Msisdn,         // Mobile number of the recipient
    string ServiceCode,    // USSD service code
    string UssdString      // USSD message content
);
