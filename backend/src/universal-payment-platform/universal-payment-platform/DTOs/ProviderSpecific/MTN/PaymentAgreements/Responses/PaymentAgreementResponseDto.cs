public record PaymentAgreementResponseDto(
    string StatusCode,
    string StatusMessage,
    string TransactionId,
    string SequenceNo,
    List<PromiseDetailDto> Data,
    LinkDto Links
);
