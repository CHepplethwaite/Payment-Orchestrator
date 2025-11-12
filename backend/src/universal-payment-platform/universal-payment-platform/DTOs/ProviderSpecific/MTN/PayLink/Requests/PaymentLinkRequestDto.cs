using universal_payment_platform.DTOs.ProviderSpecific.MTN.PayLink.Requests;

public record PaymentLinkRequestDto(
    string Channel,
    string QuoteId,
    string Description,
    string AuthenticationType,
    string? CallbackUrl,
    string? RedirectUrl,
    string DeliveryMethod,
    PayerDto Payer,
    bool IncludePayerCharges,
    List<string>? PaymentMethods,
    MoneyDto TotalAmount,
    List<ItemDetailDto> ItemDetails
);
