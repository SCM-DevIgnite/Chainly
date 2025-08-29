using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainly.Data.OTP_Validation
{
    public class ValidateOtpRequest
    {
        public string Email { get; set; }
        public string Otp { get; set; }
    }
}
