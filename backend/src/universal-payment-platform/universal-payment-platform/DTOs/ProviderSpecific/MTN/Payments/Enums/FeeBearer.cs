using System.Text.Json.Serialization;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.Payments.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FeeBearer
    {
        Payer,
        Payee
    }
}
