using System.Text.Json.Serialization;
using universal_payment_platform.DTOs.ProviderSpecific.MTN.Shared;


namespace universal_payment_platform.DTOs.ProviderSpecific.MTN.History.Responses
{
    public record AccountDto
    {
        [JsonPropertyName("amount")]
        public MonetaryTypeDto? Amount { get; init; }
    }
}
