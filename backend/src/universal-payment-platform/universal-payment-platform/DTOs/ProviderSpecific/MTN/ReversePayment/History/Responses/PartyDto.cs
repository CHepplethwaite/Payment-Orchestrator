using System.Text.Json.Serialization;

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
    public MonetaryDto Amount { get; init; }

    [JsonPropertyName("fee")]
    public MonetaryDto Fee { get; init; }

    [JsonPropertyName("externalFee")]
    public MonetaryDto ExternalFee { get; init; }

    [JsonPropertyName("discount")]
    public MonetaryDto Discount { get; init; }

    [JsonPropertyName("promotion")]
    public MonetaryDto Promotion { get; init; }

    [JsonPropertyName("loyaltyFee")]
    public MonetaryDto LoyaltyFee { get; init; }

    [JsonPropertyName("loyaltyReward")]
    public MonetaryDto LoyaltyReward { get; init; }

    [JsonPropertyName("promotionRefund")]
    public MonetaryDto PromotionRefund { get; init; }

    [JsonPropertyName("availableBalance")]
    public MonetaryDto AvailableBalance { get; init; }

    [JsonPropertyName("totalBalance")]
    public MonetaryDto TotalBalance { get; init; }

    [JsonPropertyName("committedBalance")]
    public MonetaryDto CommittedBalance { get; init; }
}