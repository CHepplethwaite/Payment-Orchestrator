public record UssdOutboundResponseDto(
    string StatusCode,            // e.g., 0000
    string StatusMessage,         // Description of the transaction
    string TransactionId,         // API transaction id
    UssdOutboundDataDto Data,
    LinkDto _Links                // Reuse generic link DTO
);
