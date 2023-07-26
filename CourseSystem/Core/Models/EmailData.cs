using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class EmailData
    {
        public List<string> To { get; }
        public string? From { get; }
        public string? DisplayName { get; }
        public string Subject { get; }
        public string? Body { get; }
        public EmailData(List<string> to, string subject, string? body = null, string? from = null, string? displayName = null)
        {
            // Receiver
            To = to;

            // Sender
            From = from;
            DisplayName = displayName;

            // Content
            Subject = subject;
            Body = body;
        }
    }
}
