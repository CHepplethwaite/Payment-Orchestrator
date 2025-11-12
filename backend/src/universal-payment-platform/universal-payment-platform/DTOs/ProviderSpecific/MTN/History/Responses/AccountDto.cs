using universal_payment_platform.DTOs.ProviderSpecific.MTN.History.Responses;
using System.Text.Json.Serialization;

namespace Application.DTOs.Payments.Responses
{
    public record AccountDto
    {
        [JsonPropertyName("amount")]
        public MoneyDto? Amount { get; init; }
    }
}
