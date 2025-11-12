using universal_payment_platform.DTOs.ProviderSpecific.MTN.Messages.SMS.DeliveryStatus.Responses;
using System.Collections.Generic;

public record SmsDeliveryStatusDataDto(
    string RequestId,                                  // Request ID returned during sending
    string ClientCorrelator,                          // Client-provided correlator
    List<SmsDeliveryStatusItemDto> DeliveryStatus,    // Array of status objects
    LinkDto Links                                     // _link object for self-reference
);
