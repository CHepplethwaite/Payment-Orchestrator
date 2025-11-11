namespace Application.DTOs.Payments.Responses
{
    public class PaymentRecordDto
    {
        public string? RecordId { get; set; }
        public string? Description { get; set; }
        public DateTime? RecordDate { get; set; }
        public MoneyDto? Amount { get; set; }
        public string? Status { get; set; }
    }
}
