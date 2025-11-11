using universal_payment_platform.DTOs.ProviderSpecific.MTN.ReversePayment.History.Responses;

public class ReversePaymentDataDto
{
    public string TransactionStatus { get; set; }
    public string TransferType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime CommitDate { get; set; }
    public string FxRate { get; set; }
    public string ExternalFxRate { get; set; }
    public string InitiatingUser { get; set; }
    public string RealUser { get; set; }
    public string ReviewingUser { get; set; }
    public string InitiatingAccountHolder { get; set; }
    public string RealAccountHolder { get; set; }
    public string ProviderCategory { get; set; }
    public PartyDto From { get; set; }
    public PartyDto To { get; set; }
    public MonetaryDto OriginalAmount { get; set; }
    public MonetaryDto ExternalAmount { get; set; }
    public MonetaryDto Amount { get; set; }
    public MonetaryDto GeneratedAmount { get; set; }
    public MonetaryDto ConsumedAmount { get; set; }
    public MonetaryDto NewBalance { get; set; }
    public List<MonetaryDto> Fees { get; set; } // optional, from/to fees/refunds
    public string TransactionText { get; set; }
    public string MainInstructionId { get; set; }
    public string InstructionId { get; set; }
    public string ExternalTransactionId { get; set; }
    public LoyaltyInformationDto LoyaltyInformation { get; set; }
}
