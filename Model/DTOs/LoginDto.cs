using System.ComponentModel.DataAnnotations;

namespace api.Model.DTOs
{
    public class LoginDto
    {
        [Required]
        [Phone]
        public string? PhoneNumber { get; set; }

        [Required]
        [DataType(DataType.Password)] 
        [StringLength(100, MinimumLength = 8)] 
        public string? Password { get; set; }
    }
}
