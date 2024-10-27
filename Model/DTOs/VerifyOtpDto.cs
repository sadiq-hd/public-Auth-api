using System.ComponentModel.DataAnnotations;

namespace api.Model.DTOs
{
    public class VerifyOtpDto
    {
        [Required]
        public string? OtpCode { get; set; } // رمز OTP

        [Required]
        public string? PhoneNumber { get; set; } // رقم الهاتف
    }
}
