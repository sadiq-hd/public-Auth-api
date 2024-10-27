using api.Model.DTOs;
using api.Services;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using api.Model;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IOtpService _otpService;
        private readonly IUserService _userService;
        private readonly UserManager<Users> _userManager;

        private static readonly Dictionary<string, RegisterDto> _pendingRegistrations = new();

        public AuthController(IOtpService otpService, IUserService userService, UserManager<Users> userManager)
        {
            _otpService = otpService;
            _userService = userService;
            _userManager = userManager;
        }

        private string GenerateJwtToken(string phoneNumber)
        {
            return $"dummy_jwt_token_for_{phoneNumber}";
        }

        // Register user with OTP
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return Problem(
                    detail: "Invalid data provided.",
                    statusCode: 400,
                    title: "Bad Request"
                );
            }

            // Check if the phone number is already registered
            var userExistenceCheck = await _userManager.Users.AnyAsync(u => u.PhoneNumber == registerDto.PhoneNumber);
            if (userExistenceCheck)
            {
                return Problem(
                    detail: "Phone number already registered.",
                    statusCode: 400,
                    title: "Bad Request"
                );
            }

            var existingUserByEmail = await _userService.GetUserByEmail(registerDto.Email);
            if (existingUserByEmail != null)
            {
                return Problem(
                    detail: "Email already registered.",
                    statusCode: 400,
                    title: "Bad Request"
                );
            }

            // Store pending registration data and send OTP
            _pendingRegistrations[registerDto.PhoneNumber] = registerDto;
            string otp = _otpService.GenerateOtp(registerDto.PhoneNumber);

            Console.WriteLine($"Registered phone number {registerDto.PhoneNumber} with OTP {otp} for verification.");
            return Ok(new { Message = "OTP sent. Please verify your phone number.", Otp = otp });
        }

        [HttpPost("verify-registration-otp")]
        public async Task<IActionResult> VerifyRegistrationOtp([FromBody] VerifyOtpDto verifyOtpDto)
        {
            Console.WriteLine("=== Starting Registration OTP Verification ===");
            Console.WriteLine($"Received Phone Number: '{verifyOtpDto.PhoneNumber}'");
            Console.WriteLine($"Received OTP Code: '{verifyOtpDto.OtpCode}'");

            if (!ModelState.IsValid)
            {
                return Problem(
                    detail: "Invalid data provided.",
                    statusCode: 400,
                    title: "Bad Request"
                );
            }

            // Check if there's a pending registration for this phone number
            if (!_pendingRegistrations.ContainsKey(verifyOtpDto.PhoneNumber))
            {
                return Problem(
                    detail: "Pending registration not found.",
                    statusCode: 400,
                    title: "Bad Request"
                );
            }

            // Verify OTP
            bool otpVerified = _otpService.VerifyOtp(verifyOtpDto.PhoneNumber, verifyOtpDto.OtpCode);
            if (!otpVerified)
            {
                return Problem(
                    detail: "Invalid or expired OTP.",
                    statusCode: 400,
                    title: "Bad Request"
                );
            }

            // Retrieve pending registration data
            RegisterDto registerDto = _pendingRegistrations[verifyOtpDto.PhoneNumber];

            // Create user after successful OTP verification
            var user = new Users
            {
                PhoneNumber = registerDto.PhoneNumber,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                UserName = registerDto.PhoneNumber // Set UserName to PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                return Problem(
                    detail: "Error creating user.",
                    statusCode: 500,
                    title: "Server Error"
                );
            }

            _pendingRegistrations.Remove(verifyOtpDto.PhoneNumber);
            return Ok(new { Message = "Registration completed successfully." });
        }

        // Login using OTP
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return Problem(
                    detail: "Invalid data provided.",
                    statusCode: 400,
                    title: "Bad Request"
                );
            }

            // Find user by phone number
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == loginDto.PhoneNumber);
            if (user == null)
            {
                return Problem(
                    detail: "Phone number not found.",
                    statusCode: 404,
                    title: "Not Found"
                );
            }

            // Check password validity
            var passwordCheck = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!passwordCheck)
            {
                return Problem(
                    detail: "Incorrect password.",
                    statusCode: 400,
                    title: "Unauthorized"
                );
            }

            // If password is correct, send OTP
            string otp = _otpService.GenerateOtp(loginDto.PhoneNumber);
            return Ok(new { Message = "OTP sent successfully.", Otp = otp });
        }

        // Verify OTP for login
        [HttpPost("verify-login-otp")]
        public async Task<IActionResult> VerifyLoginOtp([FromBody] VerifyOtpDto verifyOtpDto)
        {
            if (!ModelState.IsValid)
            {
                return Problem(
                    detail: "Invalid data provided.",
                    statusCode: 400,
                    title: "Bad Request"
                );
            }

            // Check if user exists
            var user = await _userService.GetUser(verifyOtpDto.PhoneNumber);
            if (user == null)
            {
                return Problem(
                    detail: "Phone number not found.",
                    statusCode: 404,
                    title: "Not Found"
                );
            }

            // Verify OTP
            bool otpVerified = _otpService.VerifyOtp(verifyOtpDto.PhoneNumber, verifyOtpDto.OtpCode);
            if (!otpVerified)
            {
                return Problem(
                    detail: "Invalid or expired OTP.",
                    statusCode: 400,
                    title: "Bad Request"
                );
            }

            // Generate token for the user
            var token = GenerateJwtToken(user.PhoneNumber);
            return Ok(new { Token = token });
        }

        // Forgot password - request OTP
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            // Check if user exists by phone number
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == forgotPasswordDto.PhoneNumber);
            if (user == null)
            {
                return Problem(
                    detail: "Phone number not found.",
                    statusCode: 404,
                    title: "Not Found"
                );
            }

            // Generate and store OTP temporarily
            string otp = _otpService.GenerateOtp(forgotPasswordDto.PhoneNumber);
            return Ok(new { Message = "OTP sent successfully.", Otp = otp });
        }

        // Reset password using OTP
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return Problem(
                    detail: "Invalid data provided.",
                    statusCode: 400,
                    title: "Bad Request"
                );
            }

            // Verify OTP
            if (!_otpService.VerifyOtp(resetPasswordDto.PhoneNumber, resetPasswordDto.OtpCode))
            {
                return Problem(
                    detail: "Invalid or expired OTP.",
                    statusCode: 400,
                    title: "Bad Request"
                );
            }

            // Find user by phone number
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == resetPasswordDto.PhoneNumber);
            if (user == null)
            {
                return Problem(
                    detail: "Phone number not found.",
                    statusCode: 404,
                    title: "Not Found"
                );
            }

            // Remove old password if exists
            if (await _userManager.HasPasswordAsync(user))
            {
                var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                if (!removePasswordResult.Succeeded)
                {
                    var errorMessages = string.Join(", ", removePasswordResult.Errors.Select(e => e.Description));
                    return Problem(
                        detail: $"Failed to remove old password. Errors: {errorMessages}",
                        statusCode: 500,
                        title: "Server Error"
                    );
                }
            }

            // Set new password
            var addPasswordResult = await _userManager.AddPasswordAsync(user, resetPasswordDto.NewPassword);
            if (!addPasswordResult.Succeeded)
            {
                var errorMessages = string.Join(", ", addPasswordResult.Errors.Select(e => e.Description));
                return Problem(
                    detail: $"Failed to set new password. Errors: {errorMessages}",
                    statusCode: 500,
                    title: "Server Error"
                );
            }

            return Ok(new { Message = "Password reset successfully." });
        }

        // Get user by ID
        [HttpGet("user/id/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserById(id);
            if (user == null)
            {
                return Problem(
                    detail: "User not found.",
                    statusCode: 404,
                    title: "Not Found"
                );
            }

            return Ok(user);
        }

        // Delete user by ID
        [HttpDelete("user/id/{id}")]
        public async Task<IActionResult> DeleteUserById(int id)
        {
            var result = await _userService.DeleteUserById(id);
            if (!result.Success)
            {
                return Problem(
                    detail: result.Message,
                    statusCode: 404,
                    title: "User Deletion Error"
                );
            }

            return Ok(new { Message = result.Message });
        }
    }
}
