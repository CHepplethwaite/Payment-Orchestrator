using System.Text.Json.Serialization;

namespace universal_payment_platform.DTOs.ProviderSpecific.MTN.Shared.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FeeBearer
    {
        Payer,
        Payee
    }
}
