﻿using Microsoft.AspNetCore.Http;
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
        public string? DisplayName { get; }
        public string Subject { get; }
        public string? Body { get; }
        public IFormFile? Attachment { get; set; }

        public EmailData(List<string> to, string subject, string? body = null, string? displayName = null, IFormFile? attachment = null)
        {
            // Receiver
            To = to;

            // Sender
            DisplayName = displayName;

            // Content
            Subject = subject;
            Body = body;
            Attachment = attachment;
        }
    }
}