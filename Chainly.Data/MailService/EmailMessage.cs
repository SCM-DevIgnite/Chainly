using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainly.Data.MailService
{
    public class EmailMessage
    {
        public List<MailboxAddress> To { get; set; } = new List<MailboxAddress>();
        public string Subject { get; set; }

        public string Content { get; set; }


        public EmailMessage(IEnumerable<string> to, string subject, string content)
        {
            To.AddRange(to.Select(x => new MailboxAddress("Hello", x)));
            Subject = subject;
            Content = content;
        }
    }
}
