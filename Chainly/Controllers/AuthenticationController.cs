using Chainly.Data;
using Chainly.Data.Dtos;
using Chainly.Data.MailService;
using Chainly.Data.Models;
using Chainly.Data.OTP_Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Chainly.Data.Constants;

namespace Chainly.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;
        private readonly IEmailSender _emailSender;

        public AuthenticationController(UserManager<User> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            IConfiguration config,
            AppDbContext context,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
            _context = context;
            _emailSender = emailSender;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Name.ToLower() == model.CompanyName.ToLower());

            if (company == null)
            {
                company = new Company
                {
                    Name = model.CompanyName,
                    LocationLatitude = model.LocationLatitude,
                    LocationLongitude = model.LocationLongitude,
                    Logo = null
                };
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();
            }


            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                CompanyId = company.Id
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);


            string role;


            var usersInCompany = await _userManager.Users.CountAsync(u => u.CompanyId == company.Id);

            if (usersInCompany == 1)
            {
                role = "Manager";
            }
            else
            {
                if (!string.IsNullOrEmpty(model.Role))
                {
                    var currentUser = await _userManager.GetUserAsync(User);
                    var currentRoles = await _userManager.GetRolesAsync(currentUser);
                    if (currentRoles.Contains("Manager") && (model.Role == "Manager" || model.Role == "Employee"))
                        role = model.Role;
                    else
                        role = "Employee";
                }
                else
                    role = "Employee";
            }


            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole<int>(role));
            }

            await _userManager.AddToRoleAsync(user, role);

            return Ok(new { Message = "User registered successfully", Role = role, Company = company.Name });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var token = GenerateJwtToken(user);
                return Ok(new { Token = token });
            }

            return Unauthorized("Invalid login attempt");
        }

        [HttpPost("forget-password")]
        public async Task<IActionResult> GenerateOTP([FromBody] GenerateOtpRequest otpRequest)
        {
            var user = await _userManager.FindByEmailAsync(otpRequest.Email);
            if (user == null)
                return NotFound("User Not Found !");

            var code = new Random().Next(100000, 999999).ToString();
            user.VerificationCode = code;
            user.ExpirationCode = DateTime.UtcNow.AddMinutes(10);

            await _userManager.UpdateAsync(user);

            var message = new EmailMessage(
                new List<string> { otpRequest.Email },
                "OTP For Reset Password",
                HtmlTemplate.GetVerificationCodeEmailTemplate(code)
            );

            await _emailSender.SendEmailAsync(message);

            return Ok("OTP Sent Successfully :)");
        }

        [HttpPost("validate-otp")]
        public async Task<IActionResult> ValidateOtp([FromBody] ValidateOtpRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return NotFound("User Not Found !");

            if (user.VerificationCode == request.Otp && user.ExpirationCode > DateTime.UtcNow)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                user.VerificationCode = null;
                user.ExpirationCode = null;
                await _userManager.UpdateAsync(user);
                return Ok(new { Message = "OTP Verified Successfully !", Token = token });
            }

            return BadRequest("Invalid OR Expired OTP.");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) return NotFound("User Not Found !");

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.newPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Password reset successfully!");
        }

        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return NotFound("User Not Found !");

            var newCode = new Random().Next(100000, 999999).ToString();
            user.VerificationCode = newCode;
            user.ExpirationCode = DateTime.UtcNow.AddMinutes(10);

            await _userManager.UpdateAsync(user);

            var htmlBody = HtmlTemplate.GetVerificationCodeEmailTemplate(newCode);

            var message = new EmailMessage(
                new List<string> { user.Email },
                "Your New Verification Code",
                htmlBody
            );

            await _emailSender.SendEmailAsync(message);

            return Ok("OTP Resent Successfully :)");
        }


        private async Task<string> GenerateJwtToken(User user)
        {
            var jwtSettings = _config.GetSection("Jwt");

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("companyId", user.CompanyId.ToString())
            };


            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));

                claims.AddRange((await _roleManager.GetClaimsAsync(await _roleManager.FindByNameAsync(role)))
                    .Where(c => c.Type == CustomClaimTypes.Permission)
                    .Select(c => new Claim(CustomClaimTypes.Permission, c.Value)));
            }


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToInt32(jwtSettings["DurationInMinutes"])),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}