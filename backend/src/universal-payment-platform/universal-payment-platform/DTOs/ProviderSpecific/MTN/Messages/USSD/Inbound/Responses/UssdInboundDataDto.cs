public record UssdInboundDataDto(
    string InboundResponse,   // Response value, e.g., null
    bool UserInputRequired,   // Is user input required
    int MessageType,          // MessageType for response
    string ServiceCode,       // Echoed service code
    string Msisdn             // Recipient msisdn
);
