using System.ComponentModel.DataAnnotations;

namespace api.Model.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
    }

    public class ResetPasswordDto
    {
        public string? PhoneNumber { get; set; }
        public string? OtpCode { get; set; }
        public string? NewPassword { get; set; }
    }

    public class ForgotPasswordDto
    {
        public string? PhoneNumber { get; set; }
        public string? OtpCode { get; set; }


    }
}