namespace Application.DTOs.Payments.Requests
{
    public class PaymentHistoryRequestDto
    {
        public string Id { get; set; } = string.Empty;
        public string? EndDate { get; set; }
        public string? IdType { get; set; }
        public string? NodeId { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
        public string? QueryType { get; set; }
        public string? RegistrationChannel { get; set; }
        public string? RequestType { get; set; }
        public string? Segment { get; set; }
        public string? StartDate { get; set; }
        public string? StartTime { get; set; }
        public string? Status { get; set; }
        public string? TargetSystem { get; set; }
        public string? TraceId { get; set; }

        // Headers
        public string? XAuthorization { get; set; }
        public string? TransactionId { get; set; }
    }
}
