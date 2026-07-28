// InMemoryOtpService.cs
using System.Collections.Concurrent;
using System.Security.Cryptography;
using TaskMind.Applications.Auths.Interfaces;

namespace TaskMind.Applications.Auths.Services
{
    /// <summary>
    /// Lưu OTP tạm trong bộ nhớ tiến trình (mục 4.1 - quên mật khẩu).
    /// TODO: chỉ phù hợp single-instance; khi scale nhiều instance cần thay bằng cache phân tán (Redis...) ở Infrastructure.
    /// </summary>
    public class InMemoryOtpService : IOtpService
    {
        private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(5);
        private readonly ConcurrentDictionary<string, (string Code, DateTime ExpiresAtUtc)> _store = new();

        public Task<string> GenerateOtpAsync(string email, CancellationToken cancellationToken = default)
        {
            var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            _store[Normalize(email)] = (code, DateTime.UtcNow.Add(OtpLifetime));
            return Task.FromResult(code);
        }

        public Task<bool> PeekOtpAsync(string email, string otpCode, CancellationToken cancellationToken = default)
            => Task.FromResult(IsValid(email, otpCode));

        public Task<bool> ValidateAndConsumeOtpAsync(string email, string otpCode, CancellationToken cancellationToken = default)
        {
            var valid = IsValid(email, otpCode);
            if (valid) _store.TryRemove(Normalize(email), out _);
            return Task.FromResult(valid);
        }

        private bool IsValid(string email, string otpCode)
        {
            var key = Normalize(email);
            if (!_store.TryGetValue(key, out var entry)) return false;
            if (entry.ExpiresAtUtc < DateTime.UtcNow)
            {
                _store.TryRemove(key, out _);
                return false;
            }
            return string.Equals(entry.Code, otpCode, StringComparison.Ordinal);
        }

        private static string Normalize(string email) => email.Trim().ToLowerInvariant();
    }
}