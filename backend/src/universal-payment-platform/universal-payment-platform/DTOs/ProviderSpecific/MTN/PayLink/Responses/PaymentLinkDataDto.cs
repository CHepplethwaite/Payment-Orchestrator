using universal_payment_platform.DTOs.ProviderSpecific.MTN.PayLink.Responses;

public class PaymentLinkDataDto
{
    public string ProviderTransactionId { get; set; }
    public string OrderRedirectUrl { get; set; }
    public LinkDto Links { get; set; }
}
