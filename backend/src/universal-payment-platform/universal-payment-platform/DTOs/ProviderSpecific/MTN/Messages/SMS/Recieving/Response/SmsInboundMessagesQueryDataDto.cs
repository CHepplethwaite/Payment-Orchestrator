public record SmsInboundMessagesQueryDataDto(
    string NumberOfMessagesInThisBatch,
    string TotalNumberOfPendingMessages,
    List<InboundSmsMessageDto> InboundSmsMessage,
    LinkDto Links
);
