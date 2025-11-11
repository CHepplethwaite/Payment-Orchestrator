using universal_payment_platform.DTOs.ProviderSpecific.MTN.History.Responses;

namespace Application.DTOs.Payments.Responses
{
    public class PaymentHistoryResponseDto
    {
        public string StatusCode { get; set; } = string.Empty;
        public string StatusMessage { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string SequenceNo { get; set; } = string.Empty;
        public PaymentHistoryDataDto? Data { get; set; }
    }
}
