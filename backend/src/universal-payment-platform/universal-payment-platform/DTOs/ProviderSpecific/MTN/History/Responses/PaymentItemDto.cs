namespace Application.DTOs.Payments.Responses
{
    public class PaymentItemDto
    {
        public MoneyDto? TotalAmount { get; set; }
        public string? Type { get; set; }
    }
}
