using System.Text.Json.Serialization;

namespace Application.DTOs.Payments.Requests
{
    public record PaymentHistoryRequestDto
    {
        [JsonPropertyName("id")]
        public string Id { get; init; }

        [JsonPropertyName("endDate")]
        public string? EndDate { get; init; }

        [JsonPropertyName("idType")]
        public string? IdType { get; init; }

        [JsonPropertyName("nodeId")]
        public string? NodeId { get; init; }

        [JsonPropertyName("pageNumber")]
        public int? PageNumber { get; init; }

        [JsonPropertyName("pageSize")]
        public int? PageSize { get; init; }

        [JsonPropertyName("queryType")]
        public string? QueryType { get; init; }

        [JsonPropertyName("registrationChannel")]
        public string? RegistrationChannel { get; init; }

        [JsonPropertyName("requestType")]
        public string? RequestType { get; init; }

        [JsonPropertyName("segment")]
        public string? Segment { get; init; }

        [JsonPropertyName("startDate")]
        public string? StartDate { get; init; }

        [JsonPropertyName("startTime")]
        public string? StartTime { get; init; }

        [JsonPropertyName("status")]
        public string? Status { get; init; }

        [JsonPropertyName("targetSystem")]
        public string? TargetSystem { get; init; }

        [JsonPropertyName("traceId")]
        public string? TraceId { get; init; }

        [JsonPropertyName("xAuthorization")]
        public string? XAuthorization { get; init; }

        [JsonPropertyName("transactionId")]
        public string? TransactionId { get; init; }
    }
}
