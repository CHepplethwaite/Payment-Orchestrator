using System.Collections.Generic;
using System.Text.Json.Serialization;
using universal_payment_platform.DTOs.ProviderSpecific.MTN.PayLink.Requests;

public record ItemDetailDto
{
    [JsonPropertyName("itemName")]
    public string ItemName { get; init; }

    [JsonPropertyName("itemDescription")]
    public string ItemDescription { get; init; }

    [JsonPropertyName("itemValue")]
    public string ItemValue { get; init; }

    [JsonPropertyName("currency")]
    public string Currency { get; init; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; init; }

    [JsonPropertyName("additionalInformation")]
    public List<AdditionalInformationDto>? AdditionalInformation { get; init; }
}
