using System.Text.Json.Serialization;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.Payments.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PaymentMethodType
    {
        BankCard,
        TokenizedCard,
        BankAccountDebit,
        BankAccountTransfer,
        Account,
        LoyaltyAccount,
        Bucket,
        Voucher,
        DigitalWallet,
        Airtime,
        MobileMoney,
        Invoice
    }
}
