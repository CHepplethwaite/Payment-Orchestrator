using Application.DTOs.Payments.Responses;

public class FeeCheckRequestDto
{
    public string CorrelatorId { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Name { get; set; }
    public string CallingSystem { get; set; }
    public string TransactionType { get; set; } // FeeCheck
    public string TargetSystem { get; set; }
    public string CallbackUrl { get; set; }
    public string QuoteId { get; set; }
    public string Channel { get; set; }
    public string Description { get; set; }
    public string AuthorizationCode { get; set; }
    public string FeeBearer { get; set; } // Payer/Payee
    public MonetaryDto Amount { get; set; }
    public MonetaryDto TaxAmount { get; set; }
    public MonetaryDto TotalAmount { get; set; }
    public PayerDto Payer { get; set; }
    public List<PayeeDto> Payees { get; set; }
    public PaymentMethodDto PaymentMethod { get; set; }
    public List<AdditionalInformationDto> AdditionalInformation { get; set; }
    public string Segment { get; set; } // subscriber, agent, merchant, admin
    public bool IncludePayerCharges { get; set; }
}
