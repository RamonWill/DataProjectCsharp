using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataProjectCsharp.Services.Email
{
    public class Message
    {
        public List<MailboxAddress> Recipients { get; set; }
        public string Subject { get; set; }
        public string MailContent { get; set; }

        public Message(IEnumerable<string> to, string subject, string mailContent)
        {
            Recipients = new List<MailboxAddress>();
            Recipients.AddRange(to.Select(m => new MailboxAddress(m)));
            this.Subject = subject;
            this.MailContent = mailContent;
        }
    }
}
