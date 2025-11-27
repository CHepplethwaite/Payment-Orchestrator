using System.Text.Json.Serialization;
using universal_payment_platform.DTOs.ProviderSpecific.MTN.Shared;

public record ReversePaymentDataDto
{
    [JsonPropertyName("transactionStatus")]
    public string? TransactionStatus { get; init; }

    [JsonPropertyName("transferType")]
    public string? TransferType { get; init; }

    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; init; }

    [JsonPropertyName("commitDate")]
    public DateTime CommitDate { get; init; }

    [JsonPropertyName("fxRate")]
    public string? FxRate { get; init; }

    [JsonPropertyName("externalFxRate")]
    public string? ExternalFxRate { get; init; }

    [JsonPropertyName("initiatingUser")]
    public string? InitiatingUser { get; init; }

    [JsonPropertyName("realUser")]
    public string? RealUser { get; init; }

    [JsonPropertyName("reviewingUser")]
    public string? ReviewingUser { get; init; }

    [JsonPropertyName("initiatingAccountHolder")]
    public string? InitiatingAccountHolder { get; init; }

    [JsonPropertyName("realAccountHolder")]
    public string? RealAccountHolder { get; init; }

    [JsonPropertyName("providerCategory")]
    public string? ProviderCategory { get; init; }

    [JsonPropertyName("from")]
    public PartyDto? From { get; init; }

    [JsonPropertyName("to")]
    public PartyDto? To { get; init; }

    [JsonPropertyName("originalAmount")]
    public MonetaryTypeDto? OriginalAmount { get; init; }

    [JsonPropertyName("externalAmount")]
    public MonetaryTypeDto? ExternalAmount { get; init; }

    [JsonPropertyName("amount")]
    public MonetaryTypeDto? Amount { get; init; }

    [JsonPropertyName("generatedAmount")]
    public MonetaryTypeDto? GeneratedAmount { get; init; }

    [JsonPropertyName("consumedAmount")]
    public MonetaryTypeDto? ConsumedAmount { get; init; }

    [JsonPropertyName("newBalance")]
    public MonetaryTypeDto? NewBalance { get; init; }

    [JsonPropertyName("fees")]
    public List<MonetaryTypeDto>? Fees { get; init; }

    [JsonPropertyName("transactionText")]
    public string? TransactionText { get; init; }

    [JsonPropertyName("mainInstructionId")]
    public string? MainInstructionId { get; init; }

    [JsonPropertyName("instructionId")]
    public string? InstructionId { get; init; }

    [JsonPropertyName("externalTransactionId")]
    public string? ExternalTransactionId { get; init; }

    [JsonPropertyName("loyaltyInformation")]
    public LoyaltyInformationDto? LoyaltyInformation { get; init; }
}