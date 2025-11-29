using System.Collections.Generic;

namespace universal_payment_platform.Infrastructure.Email.Models
{
    public class EmailMessage
    {
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsHtml { get; set; } = true;
        public string From { get; set; } = string.Empty;
        public List<EmailAttachment> Attachments { get; set; } = [];
    }

    public class EmailAttachment
    {
        public string FileName { get; set; } = string.Empty;
        public byte[] Content { get; set; } = [];
        public string ContentType { get; set; } = "application/octet-stream";
    }
}