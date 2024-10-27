namespace api.Services.Interfaces
{
    public interface IOtpService
    {
        string GenerateOtp(string phoneNumber);
        bool VerifyOtp(string phoneNumber, string otp);
    }
}
