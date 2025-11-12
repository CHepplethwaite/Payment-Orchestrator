public record UssdInboundResponseDto(
    string StatusCode,         // e.g., 0000
    string StatusMessage,      // Description of the transaction
    string TransactionId,      // API transaction id
    UssdInboundDataDto Data,
    LinkDto Links              // Reuse generic link DTO
);
