using System.Text.Json.Serialization;
using universal_payment_platform.DTOs.ProviderSpecific.MTN.Shared;

public record PartyDto
{
    [JsonPropertyName("fri")]
    public string FRI { get; init; }

    [JsonPropertyName("account")]
    public string Account { get; init; }

    [JsonPropertyName("accountHolder")]
    public string AccountHolder { get; init; }

    [JsonPropertyName("firstName")]
    public string FirstName { get; init; }

    [JsonPropertyName("lastName")]
    public string LastName { get; init; }

    [JsonPropertyName("handlerFirstName")]
    public string HandlerFirstName { get; init; }

    [JsonPropertyName("handlerLastName")]
    public string HandlerLastName { get; init; }

    [JsonPropertyName("posMsisdn")]
    public string PosMsisdn { get; init; }

    [JsonPropertyName("note")]
    public string Note { get; init; }

    [JsonPropertyName("amount")]
    public MonetaryTypeDto Amount { get; init; }

    [JsonPropertyName("fee")]
    public MonetaryTypeDto Fee { get; init; }

    [JsonPropertyName("externalFee")]
    public MonetaryTypeDto? ExternalFee { get; init; }

    [JsonPropertyName("discount")]
    public MonetaryTypeDto? Discount { get; init; }

    [JsonPropertyName("promotion")]
    public MonetaryTypeDto? Promotion { get; init; }

    [JsonPropertyName("loyaltyFee")]
    public MonetaryTypeDto? LoyaltyFee { get; init; }

    [JsonPropertyName("loyaltyReward")]
    public MonetaryTypeDto? LoyaltyReward { get; init; }

    [JsonPropertyName("promotionRefund")]
    public MonetaryTypeDto? PromotionRefund { get; init; }

    [JsonPropertyName("availableBalance")]
    public MonetaryTypeDto? AvailableBalance { get; init; }

    [JsonPropertyName("totalBalance")]
    public MonetaryTypeDto? TotalBalance { get; init; }

    [JsonPropertyName("committedBalance")]
    public MonetaryTypeDto? CommittedBalance { get; init; }
}