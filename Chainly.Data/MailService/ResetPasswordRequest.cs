using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainly.Data.MailService
{
    public class ResetPasswordRequest
    {
        public string Email { get; set; }

        public string Token { get; set; }

        public string newPassword { get; set; }
    }
}
