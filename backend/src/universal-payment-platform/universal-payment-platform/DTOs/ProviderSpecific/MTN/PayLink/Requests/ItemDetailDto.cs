using universal_payment_platform.DTOs.ProviderSpecific.MTN.PayLink.Requests;

public class ItemDetailDto
{
    public string ItemName { get; set; }
    public string ItemDescription { get; set; }
    public string ItemValue { get; set; }
    public string Currency { get; set; }
    public int Quantity { get; set; }
    public List<AdditionalInformationDto>? AdditionalInformation { get; set; }
}
