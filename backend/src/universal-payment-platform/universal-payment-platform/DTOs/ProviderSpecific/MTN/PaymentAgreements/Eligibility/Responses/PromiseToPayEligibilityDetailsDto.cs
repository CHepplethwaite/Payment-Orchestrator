public record PromiseToPayEligibilityDetailsDto(
    bool IsEligible,
    string? Reason,
    DateTime? ValidUntil
);
