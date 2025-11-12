public record UssdOutboundDataDto(
    string OutboundResponse,   // 0 indicates success
    string SessionId,          // Echoed session id
    string Msisdn              // Mobile recipient msisdn
);
