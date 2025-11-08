using System.Text.Json.Serialization;

namespace UniversalPaymentPlatform.DTOs.ProviderSpecific.MTN
{
    public class MtnTransactionStatusResponse
    {
        [JsonPropertyName("statusCode")]
        public string StatusCode { get; set; }

        [JsonPropertyName("statusMessage")]
        public string StatusMessage { get; set; }

        [JsonPropertyName("correlatorId")]
        public string CorrelatorId { get; set; }

        [JsonPropertyName("customerId")]
        public string CustomerId { get; set; }

        [JsonPropertyName("sequenceNo")]
        public string SequenceNo { get; set; }

        [JsonPropertyName("providerTransactionId")]
        public string ProviderTransactionId { get; set; }

        [JsonPropertyName("data")]
        public MtnTransactionData Data { get; set; }

        [JsonPropertyName("customer")]
        public MtnCustomerInfo Customer { get; set; }  // This contains charges & additionalInformation

        [JsonPropertyName("_links")]
        public MtnLinks Links { get; set; }
    }

    public class MtnTransactionData
    {
        [JsonPropertyName("financialTransactionId")]
        public string FinancialTransactionId { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("requestType")]
        public string RequestType { get; set; }

        [JsonPropertyName("fulfillmentStatus")]
        public string FulfillmentStatus { get; set; }

        [JsonPropertyName("transactionRefParent")]
        public string TransactionRefParent { get; set; }

        [JsonPropertyName("transactionDescription")]
        public string TransactionDescription { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("channel")]
        public string Channel { get; set; }

        [JsonPropertyName("product")]
        public string Product { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }

    public class MtnCustomerInfo
    {
        [JsonPropertyName("charges")]
        public MtnCharges Charges { get; set; }

        [JsonPropertyName("additionalInformation")]
        public MtnAdditionalInfo AdditionalInformation { get; set; }
    }

    public class MtnCharges
    {
        [JsonPropertyName("totalAmount")]
        public decimal? TotalAmount { get; set; }
    }

    public class MtnAdditionalInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("details")]
        public MtnTransactionDetails Details { get; set; }
    }

    public class MtnTransactionDetails
    {
        [JsonPropertyName("brand")]
        public string Brand { get; set; }

        [JsonPropertyName("fulfillmentMsisdn")]
        public string FulfillmentMsisdn { get; set; }

        [JsonPropertyName("issuer")]
        public string Issuer { get; set; }
    }

    public class MtnLinks
    {
        [JsonPropertyName("self")]
        public MtnSelfLink Self { get; set; }
    }

    public class MtnSelfLink
    {
        [JsonPropertyName("href")]
        public string Href { get; set; }
    }
}