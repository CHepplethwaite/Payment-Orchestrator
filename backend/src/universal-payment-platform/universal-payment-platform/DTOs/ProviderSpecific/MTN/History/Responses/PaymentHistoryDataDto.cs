using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using universal_payment_platform.DTOs.ProviderSpecific.MTN.History.Responses;

namespace Application.DTOs.Payments.Responses
{
    public record PaymentHistoryDataDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        [JsonPropertyName("href")]
        public string? Href { get; init; }

        [JsonPropertyName("authorizationCode")]
        public string? AuthorizationCode { get; init; }

        [JsonPropertyName("correlatorId")]
        public string? CorrelatorId { get; init; }

        [JsonPropertyName("description")]
        public string? Description { get; init; }

        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("paymentDate")]
        public DateTime? PaymentDate { get; init; }

        [JsonPropertyName("status")]
        public string? Status { get; init; }

        [JsonPropertyName("statusDate")]
        public DateTime? StatusDate { get; init; }

        [JsonPropertyName("account")]
        public AccountDto? Account { get; init; }

        [JsonPropertyName("relatedParty")]
        public RelatedPartyDto? RelatedParty { get; init; }

        [JsonPropertyName("payer")]
        public PayerDto? Payer { get; init; }

        [JsonPropertyName("paymentItem")]
        public List<PaymentItemDto>? PaymentItem { get; init; }

        [JsonPropertyName("callbackUrl")]
        public string? CallbackUrl { get; init; }

        [JsonPropertyName("paymentRecords")]
        public List<PaymentRecordDto>? PaymentRecords { get; init; }
    }
}
