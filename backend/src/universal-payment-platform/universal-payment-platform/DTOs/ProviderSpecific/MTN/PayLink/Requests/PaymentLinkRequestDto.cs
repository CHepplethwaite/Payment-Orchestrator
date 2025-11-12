using System.Collections.Generic;
using System.Text.Json.Serialization;
using universal_payment_platform.DTOs.ProviderSpecific.MTN.PayLink.Requests;

public record PaymentLinkRequestDto
{
    [JsonPropertyName("channel")]
    public string Channel { get; init; }

    [JsonPropertyName("quoteId")]
    public string QuoteId { get; init; }

    [JsonPropertyName("description")]
    public string Description { get; init; }

    [JsonPropertyName("authenticationType")]
    public string AuthenticationType { get; init; }

    [JsonPropertyName("callbackUrl")]
    public string? CallbackUrl { get; init; }

    [JsonPropertyName("redirectUrl")]
    public string? RedirectUrl { get; init; }

    [JsonPropertyName("deliveryMethod")]
    public string DeliveryMethod { get; init; }

    [JsonPropertyName("payer")]
    public PayerDto Payer { get; init; }

    [JsonPropertyName("includePayerCharges")]
    public bool IncludePayerCharges { get; init; }

    [JsonPropertyName("paymentMethods")]
    public List<string>? PaymentMethods { get; init; }

    [JsonPropertyName("totalAmount")]
    public MoneyDto TotalAmount { get; init; }

    [JsonPropertyName("itemDetails")]
    public List<ItemDetailDto> ItemDetails { get; init; }
}
