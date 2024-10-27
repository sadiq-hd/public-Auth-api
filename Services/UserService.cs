using api.Model;
using api.Model.DTOs;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<Users> _passwordHasher;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<Users>();
        }

        public async Task<UserServiceResponse> CreateUser(RegisterDto registerDto)
        {
            // تحقق مما إذا كان رقم الهاتف أو البريد الإلكتروني موجودًا بالفعل
            if (await _context.Users.AnyAsync(u => u.PhoneNumber == registerDto.PhoneNumber || u.Email == registerDto.Email))
            {
                return new UserServiceResponse
                {
                    Success = false,
                    Message = "User already exists."
                };
            }

            var user = new Users
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PhoneNumber = registerDto.PhoneNumber,
                Email = registerDto.Email
            };

            // تشفير كلمة المرور باستخدام PasswordHasher
            user.PasswordHash = _passwordHasher.HashPassword(user, registerDto.Password);

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return new UserServiceResponse
            {
                Success = true,
                Message = "User created successfully"
            };
        }

        public async Task<UserDto> GetUser(string phoneNumber)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
            return user != null ? new UserDto
            {
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            } : null;
        }

        public async Task<UserDto> GetUserById(int id)
        {
            var user = await _context.Users.FindAsync(id);
            return user != null ? new UserDto
            {
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Email = user.Email ?? string.Empty
            } : null;
        }

        public async Task<UserDto?> GetUserByEmail(string? email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            return user != null ? new UserDto
            {
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Email = user.Email ?? string.Empty
            } : null;
        }


        public async Task<UserServiceResponse> ResetPassword(string phoneNumber, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
            if (user != null)
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, newPassword); // تشفير كلمة المرور الجديدة
                await _context.SaveChangesAsync();

                return new UserServiceResponse
                {
                    Success = true,
                    Message = "Password reset successfully."
                };
            }

            return new UserServiceResponse
            {
                Success = false,
                Message = "User not found."
            };
        }

        public async Task<UserServiceResponse> DeleteUser(string phoneNumber)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return new UserServiceResponse
                {
                    Success = true,
                    Message = "User deleted successfully."
                };
            }

            return new UserServiceResponse
            {
                Success = false,
                Message = "User not found."
            };
        }

        public async Task<UserServiceResponse> DeleteUserById(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return new UserServiceResponse
                {
                    Success = true,
                    Message = "User deleted successfully."
                };
            }

            return new UserServiceResponse
            {
                Success = false,
                Message = "User not found."
            };
        }

        public async Task<UserServiceResponse> UpdatePassword(string phoneNumber, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
            if (user != null)
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, newPassword); // تشفير كلمة المرور الجديدة
                await _context.SaveChangesAsync();

                return new UserServiceResponse
                {
                    Success = true,
                    Message = "Password updated successfully."
                };
            }

            return new UserServiceResponse
            {
                Success = false,
                Message = "User not found."
            };
        }
    }
}
