using universal_payment_platform.DTOs.ProviderSpecific.MTN.History.Responses;

namespace Application.DTOs.Payments.Responses
{
    public class PaymentHistoryDataDto
    {
        public string Id { get; set; } = string.Empty;
        public string Href { get; set; } = string.Empty;
        public string? AuthorizationCode { get; set; }
        public string CorrelatorId { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Name { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? Status { get; set; }
        public DateTime? StatusDate { get; set; }

        public AccountDto? Account { get; set; }
        public RelatedPartyDto? RelatedParty { get; set; }
        public PayerDto? Payer { get; set; }

        public List<PaymentItemDto>? PaymentItem { get; set; }
        public string? CallbackUrl { get; set; }

        public List<PaymentRecordDto>? PaymentRecords { get; set; }
    }
}
