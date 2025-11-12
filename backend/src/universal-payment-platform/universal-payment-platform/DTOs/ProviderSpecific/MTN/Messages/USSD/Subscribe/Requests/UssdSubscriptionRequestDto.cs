public record UssdSubscriptionRequestDto(
    string ServiceCode,    // e.g., *1234*356#
    string CallbackUrl,    // URL to receive MO messages
    string TargetSystem    // e.g., AYO
);
