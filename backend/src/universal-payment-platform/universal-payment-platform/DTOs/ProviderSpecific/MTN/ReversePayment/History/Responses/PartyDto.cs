using universal_payment_platform.DTOs.ProviderSpecific.MTN.ReversePayment.History.Responses;

public class PartyDto
{
    public string FRI { get; set; }
    public string Account { get; set; }
    public string AccountHolder { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string HandlerFirstName { get; set; }
    public string HandlerLastName { get; set; }
    public string PosMsisdn { get; set; }
    public string Note { get; set; }
    public MonetaryDto Amount { get; set; }
    public MonetaryDto Fee { get; set; }
    public MonetaryDto ExternalFee { get; set; }
    public MonetaryDto Discount { get; set; }
    public MonetaryDto Promotion { get; set; }
    public MonetaryDto LoyaltyFee { get; set; }
    public MonetaryDto LoyaltyReward { get; set; }
    public MonetaryDto PromotionRefund { get; set; }
    public MonetaryDto AvailableBalance { get; set; }
    public MonetaryDto TotalBalance { get; set; }
    public MonetaryDto CommittedBalance { get; set; }
    // Add any other monetary fields as needed
}
