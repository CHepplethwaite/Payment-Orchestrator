using System.Text.Json.Serialization;
using universal_payment_platform.DTOs.ProviderSpecific.MTN.Payments.Requests.PaymentMethodDetails;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN.Payments.Requests.PaymentMethodDetails
{
    public record PaymentMethodDetailsDto
    {
        [JsonPropertyName("bankCard")]
        public BankCardDetailsDto? BankCard { get; init; }

        [JsonPropertyName("tokenizedCard")]
        public TokenizedCardDetailsDto? TokenizedCard { get; init; }

        [JsonPropertyName("bankAccountDebit")]
        public BankAccountDebitDetailsDto? BankAccountDebit { get; init; }

        [JsonPropertyName("bankAccountTransfer")]
        public BankAccountTransferDetailsDto? BankAccountTransfer { get; init; }

        [JsonPropertyName("account")]
        public AccountDetailsDto? Account { get; init; }

        [JsonPropertyName("loyaltyAccount")]
        public LoyaltyAccountDetailsDto? LoyaltyAccount { get; init; }

        [JsonPropertyName("bucket")]
        public BucketDetailsDto? Bucket { get; init; }

        [JsonPropertyName("voucher")]
        public VoucherDetailsDto? Voucher { get; init; }

        [JsonPropertyName("digitalWallet")]
        public DigitalWalletDetailsDto? DigitalWallet { get; init; }

        [JsonPropertyName("invoice")]
        public InvoiceDetailsDto? Invoice { get; init; }
    }
}
