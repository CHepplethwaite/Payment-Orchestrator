using System.Collections.Generic;
using System.Text.Json.Serialization;

public record SmsInboundMessagesQueryDataDto
{
    [JsonPropertyName("numberOfMessagesInThisBatch")]
    public string NumberOfMessagesInThisBatch { get; init; }

    [JsonPropertyName("totalNumberOfPendingMessages")]
    public string TotalNumberOfPendingMessages { get; init; }

    [JsonPropertyName("inboundSmsMessage")]
    public List<InboundSmsMessageDto> InboundSmsMessage { get; init; }

    [JsonPropertyName("links")]
    public LinkDto Links { get; init; }
}
