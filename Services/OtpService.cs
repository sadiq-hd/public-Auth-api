using api.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace api.Services
{
    public class OtpService : IOtpService
    {
        private readonly IDistributedCache _cache;
        private readonly Random _random;
        private readonly ILogger<OtpService> _logger;

        public OtpService(IDistributedCache cache, ILogger<OtpService> logger)
        {
            _cache = cache;
            _random = new Random();
            _logger = logger;
        }

        public string GenerateOtp(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentException("رقم الهاتف غير صالح", nameof(phoneNumber));
            }

            phoneNumber = phoneNumber.Trim();
            string otp = _random.Next(100000, 999999).ToString();
            var expiryTime = DateTime.Now.AddMinutes(5);

            var otpInfo = new OtpInfo
            {
                Otp = otp,
                ExpiryTime = expiryTime
            };

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };

            var serializedOtpInfo = JsonSerializer.Serialize(otpInfo);
            _cache.SetString(GetCacheKey(phoneNumber), serializedOtpInfo, cacheOptions);

            _logger.LogInformation(
                "=== OTP Generation ===\n" +
                $"Phone Number: {phoneNumber}\n" +
                $"Generated OTP: {otp}\n" +
                $"Expiry Time: {expiryTime}\n" +
                $"Stored OTP in Distributed Cache\n" +
                "====================");

            return otp;
        }

        public bool VerifyOtp(string phoneNumber, string otp)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(otp))
            {
                _logger.LogWarning("محاولة تحقق غير صالحة - رقم الهاتف أو OTP فارغ");
                return false;
            }

            phoneNumber = phoneNumber.Trim();
            otp = otp.Trim();

            _logger.LogInformation(
                "=== OTP Verification Attempt ===\n" +
                $"Phone Number: {phoneNumber}\n" +
                $"Submitted OTP: {otp}");

            var cachedValue = _cache.GetString(GetCacheKey(phoneNumber));

            if (string.IsNullOrEmpty(cachedValue))
            {
                _logger.LogWarning("No OTP found for this phone number.");
                return false;
            }

            var otpInfo = JsonSerializer.Deserialize<OtpInfo>(cachedValue);

            if (otpInfo.ExpiryTime < DateTime.Now)
            {
                _logger.LogInformation("OTP has expired.");
                _cache.Remove(GetCacheKey(phoneNumber));
                return false;
            }

            bool isValid = otpInfo.Otp == otp;

            if (isValid)
            {
                _logger.LogInformation("OTP verification successful, removing OTP.");
                _cache.Remove(GetCacheKey(phoneNumber));
            }
            else
            {
                _logger.LogWarning("OTP verification failed - incorrect code.");
            }

            return isValid;
        }

        private string GetCacheKey(string phoneNumber) => $"OTP_{phoneNumber}";

        private class OtpInfo
        {
            public string Otp { get; set; }
            public DateTime ExpiryTime { get; set; }
        }
    }
}