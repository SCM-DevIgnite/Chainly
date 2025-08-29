using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainly.Data.MailService
{
    public interface IEmailSender
    {
        Task SendEmailAsync(EmailMessage message);
    }
}
