using universal_payment_platform.DTOs.ProviderSpecific.MTN.PayLink.Requests;

public record ItemDetailDto(
    string ItemName,
    string ItemDescription,
    string ItemValue,
    string Currency,
    int Quantity,
    List<AdditionalInformationDto>? AdditionalInformation
);

