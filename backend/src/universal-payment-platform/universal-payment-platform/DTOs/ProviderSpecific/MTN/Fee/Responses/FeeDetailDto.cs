public class FeeDetailDto
{
    public string FeeType { get; set; } // e.g., transfer fee, tax
    public MonetaryDto Amount { get; set; }
    public string Description { get; set; }
    public string Recipient { get; set; } // optional, if fee goes to provider or system
}
