using api.Model.DTOs;

namespace api.Services.Interfaces
{
    public class UserServiceResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public interface IUserService
    {
        Task<UserServiceResponse> CreateUser(RegisterDto registerDto);
        Task<UserDto> GetUser(string phoneNumber);
        Task<UserDto> GetUserByEmail(string email);
        Task<UserDto> GetUserById(int id); 
        Task<UserServiceResponse> DeleteUser(string phoneNumber);
        Task<UserServiceResponse> DeleteUserById(int id); 
        Task<UserServiceResponse> UpdatePassword(string phoneNumber, string newPassword);
        Task<UserServiceResponse> ResetPassword(string phoneNumber, string newPassword);

    }
   

}
