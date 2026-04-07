using AIResumeAnalyzer.MVC.Config;
using AIResumeAnalyzer.MVC.DTOs;
using AIResumeAnalyzer.MVC.Helpers;
using AIResumeAnalyzer.MVC.Models;
using AIResumeAnalyzer.MVC.Repositories;
using AIResumeAnalyzer.MVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AIResumeAnalyzer.MVC.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class ApiAuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly UserRepository _userRepository;
        private readonly SmsService _smsService;
        private readonly AdminSettings _adminSettings;

        public ApiAuthController(
            AuthService authService, 
            UserRepository userRepository, 
            SmsService smsService,
            IOptions<AdminSettings> adminSettings)
        {
            _authService = authService;
            _userRepository = userRepository;
            _smsService = smsService;
            _adminSettings = adminSettings.Value;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _authService.IsEmailTakenAsync(request.Email))
            {
                return BadRequest(ApiResponse<object>.Error("Email is already taken."));
            }

            var role = (request.Email.Equals(_adminSettings.AdminEmail, StringComparison.OrdinalIgnoreCase)) ? "Admin" : "User";

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                PasswordHash = _authService.HashPassword(request.Password),
                Role = role
            };

            await _userRepository.CreateUserAsync(user);

            return Ok(ApiResponse<object>.Ok(new { user.Id, user.Email, user.Role }, "Registration successful."));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userRepository.GetUserByEmailAsync(request.Email);

            if (user == null || !_authService.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized(ApiResponse<object>.Error("Invalid credentials."));
            }

            var token = _authService.GenerateJwtToken(user);

            // Set the JWT as an HttpOnly cookie so server-rendered pages can authenticate
            Response.Cookies.Append("AuthToken", token, new CookieOptions
            {
                HttpOnly = true,       // Not accessible via JavaScript (security)
                Secure = false,        // Set to true in production with HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(1), // Match JWT expiration
                Path = "/"
            });

            var response = new AuthResponse
            {
                Token = token,
                User = new { user.Id, user.Name, user.Email, user.Role }
            };

            return Ok(ApiResponse<AuthResponse>.Ok(response, "Login successful."));
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Clear the authentication cookie
            Response.Cookies.Delete("AuthToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            });

            return Ok(ApiResponse<object>.Ok(new { }, "Logged out successfully."));
        }

        [HttpPost("forgot-password-sms")]
        public async Task<IActionResult> ForgotPasswordSms([FromBody] ForgotPasswordSmsRequest request)
        {
            var user = await _userRepository.GetUserByPhoneAsync(request.Phone);
            if (user == null)
            {
                return BadRequest(ApiResponse<object>.Error("No account found with this phone number."));
            }

            var code = _authService.Generate6DigitCode();
            user.SmsResetCode = code;
            user.ResetCodeExpiration = DateTime.UtcNow.AddMinutes(15);

            await _userRepository.UpdateUserAsync(user.Id!, user);

            await _smsService.SendSmsAsync(user.Phone!, $"Your ORBIT AI password reset code is: {code}. Valid for 15 minutes.");

            return Ok(ApiResponse<object>.Ok(new { }, "Reset code sent successfully via SMS."));
        }

        [HttpPost("reset-password-sms")]
        public async Task<IActionResult> ResetPasswordSms([FromBody] ResetPasswordSmsRequest request)
        {
            var user = await _userRepository.GetUserByPhoneAsync(request.Phone);
            if (user == null || user.SmsResetCode != request.Code || user.ResetCodeExpiration < DateTime.UtcNow)
            {
                return BadRequest(ApiResponse<object>.Error("Invalid or expired reset code."));
            }

            user.PasswordHash = _authService.HashPassword(request.NewPassword);
            user.SmsResetCode = null;
            user.ResetCodeExpiration = null;

            await _userRepository.UpdateUserAsync(user.Id!, user);

            return Ok(ApiResponse<object>.Ok(new { }, "Password has been reset successfully."));
        }
    }

    public class ForgotPasswordSmsRequest
    {
        public string Phone { get; set; } = string.Empty;
    }

    public class ResetPasswordSmsRequest
    {
        public string Phone { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
