// IOtpService.cs
namespace TaskMind.Applications.Auths.Interfaces
{
    /// <summary>Sinh/kiểm tra mã OTP dùng cho quy trình quên mật khẩu (mục 4.1).</summary>
    public interface IOtpService
    {
        /// <summary>Sinh OTP mới cho email (ghi đè OTP cũ nếu có), dùng cho cả gửi lần đầu và gửi lại.</summary>
        Task<string> GenerateOtpAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>Kiểm tra OTP hợp lệ nhưng KHÔNG tiêu huỷ — dùng ở bước xác thực OTP (bước 2), để người dùng còn có thể dùng lại OTP này ở bước đặt mật khẩu (bước 3).</summary>
        Task<bool> PeekOtpAsync(string email, string otpCode, CancellationToken cancellationToken = default);

        /// <summary>Kiểm tra OTP hợp lệ và tiêu huỷ ngay (dùng 1 lần) — dùng ở bước đặt mật khẩu mới (bước 3).</summary>
        Task<bool> ValidateAndConsumeOtpAsync(string email, string otpCode, CancellationToken cancellationToken = default);
    }
}