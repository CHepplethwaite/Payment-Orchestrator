using System.Text.Json.Serialization;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.Payments.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum InvoiceFrequencyType
    {
        OnCall,
        Once,
        Hourly,
        Daily,
        Weekly,
        EveryXd // every_[1-366]d
    }
}
