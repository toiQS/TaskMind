using System;
using TaskMind.Domain.Commons.Result;

namespace TaskMind.Domain.Entities.parents
{
    public class Security
    {
        public Guid Id { get; private set; }
        public virtual Account Account { get; private set; } = default!;

        public string PasswordHash { get; private set; } = string.Empty;
        public string RefreshToken { get; private set; } = string.Empty;

        /// <summary>
        /// Thời điểm Refresh Token bị thu hồi. Null nghĩa là Token vẫn đang hoạt động bình thường.
        /// </summary>
        public DateTime? RevokeAt { get; private set; }

        /// <summary>
        /// Thuộc tính tiện ích kiểm tra xem Token đã bị thu hồi hay chưa.
        /// </summary>
        public bool IsRevoked => RevokeAt.HasValue && RevokeAt.Value <= DateTime.UtcNow;

        private Security() { }

        private Security(Guid id, string passwordHash, string refreshToken)
        {
            Id = id;
            PasswordHash = passwordHash;
            RefreshToken = refreshToken;
            RevokeAt = null; // Mặc định khi tạo mới là chưa bị thu hồi
        }

        public static Result<Security> Create(Guid id, string passwordHash, string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                return Result<Security>.Failure("Password hash cannot be null or empty.");
            }
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return Result<Security>.Failure("Refresh token cannot be null or empty.");
            }

            var security = new Security(id, passwordHash, refreshToken);
            return Result<Security>.Success(security);
        }

        public Result UpdatePassword(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
            {
                return Result.Failure("New password hash cannot be null or empty.");
            }

            PasswordHash = newPasswordHash;
            return Result.Success();
        }

        /// <summary>
        /// Cập nhật Refresh Token mới và reset lại trạng thái Revoke.
        /// </summary>
        public Result UpdateRefreshToken(string newRefreshToken)
        {
            if (string.IsNullOrWhiteSpace(newRefreshToken))
            {
                return Result.Failure("Refresh token cannot be null or empty.");
            }

            RefreshToken = newRefreshToken;
            RevokeAt = null; // Reset lại RevokeAt khi cấp token mới

            return Result.Success();
        }

        /// <summary>
        /// Hành vi thu hồi Refresh Token (ví dụ: khi User ấn Logout hoặc phát hiện bất thường).
        /// </summary>
        public Result RevokeRefreshToken()
        {
            if (IsRevoked)
            {
                return Result.Failure("Refresh token is already revoked.");
            }

            RevokeAt = DateTime.UtcNow;
            return Result.Success();
        }
    }
}