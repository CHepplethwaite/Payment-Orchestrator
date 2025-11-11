namespace Application.DTOs.Payments.Responses
{
    public class MoneyDto
    {
        public decimal Amount { get; set; }
        public string Units { get; set; } = string.Empty;
    }
}
