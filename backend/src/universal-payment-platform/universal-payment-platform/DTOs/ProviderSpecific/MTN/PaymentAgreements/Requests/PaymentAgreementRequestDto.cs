public record PaymentAgreementRequestDto(
    string BillingAccountNo,
    string ServiceName,
    DateTime PromiseOpenDate,
    double PromiseAmount,
    string NumberOfInstallments,
    string DurationUOM,
    string PromiseThreshold
);
