using universal_payment_platform.DTOs.ProviderSpecific.MTN.PayLink.Requests;

public class PaymentLinkRequestDto
{
    public string Channel { get; set; }
    public string QuoteId { get; set; }
    public string Description { get; set; }
    public string AuthenticationType { get; set; }
    public string? CallbackUrl { get; set; }
    public string? RedirectUrl { get; set; }
    public string DeliveryMethod { get; set; }
    public PayerDto Payer { get; set; }
    public bool IncludePayerCharges { get; set; }
    public List<string>? PaymentMethods { get; set; }
    public MoneyDto TotalAmount { get; set; }
    public List<ItemDetailDto> ItemDetails { get; set; }
}
