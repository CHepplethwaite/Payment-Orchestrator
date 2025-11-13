using System.Text.Json.Serialization;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.Payments.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TargetSystem
    {
        ECW,
        AYO,
        EWP,
        OCC,
        CPG,
        CELD
    }
}
