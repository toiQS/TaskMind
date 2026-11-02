using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskMind.DomainLayer.TaskMind.Domain.Commons.Result;

namespace TaskMind.DomainLayer.TaskMind.Domain.Entities
{
    public class Security
    {
        [Key, ForeignKey(nameof(Account))]
        public Guid Id { get; private set; }
        public virtual Account Account { get; private set; } = default!;

        public string PasswordHash { get; private set; } = string.Empty;
        [Column("RefreshToken")]
        public string RefreshTokenHash { get; private set; } = string.Empty;
        public string EmailVerificationTokenHash { get; private set; } = string.Empty;
        public DateTime? EmailVerificationTokenExpiresAtUtc { get; private set; }
        public string PasswordResetTokenHash { get; private set; } = string.Empty;
        public DateTime? PasswordResetTokenExpiresAtUtc { get; private set; }

        /// <summary>
        /// Thời điểm Refresh Token bị thu hồi. Null nghĩa là token chưa bị thu hồi.
        /// </summary>
        public DateTime? RevokeAt { get; private set; }
        public DateTime? RefreshTokenExpiresAtUtc { get; private set; }

        public bool IsRevoked => RevokeAt.HasValue;
        public bool IsRefreshTokenExpired =>
            RefreshTokenExpiresAtUtc.HasValue && RefreshTokenExpiresAtUtc.Value <= DateTime.UtcNow;

        private Security() { }

        private Security(Guid id, string passwordHash)
        {
            Id = id;
            PasswordHash = passwordHash;
            RefreshTokenHash = string.Empty;
            RevokeAt = null;
            RefreshTokenExpiresAtUtc = null;
        }

        public static Result<Security> Create(Guid id, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                return Result<Security>.Failure("Password hash cannot be null or empty.");
            }


            var security = new Security(id, passwordHash);
            return Result<Security>.Success(security);
        }

        public Result UpdatePassword(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
            {
                return Result.Failure("New password hash cannot be null or empty.");
            }

            PasswordHash = newPasswordHash;
            RefreshTokenHash = string.Empty;
            RefreshTokenExpiresAtUtc = null;
            RevokeAt = DateTime.UtcNow;
            PasswordResetTokenHash = string.Empty;
            PasswordResetTokenExpiresAtUtc = null;
            return Result.Success();
        }

        public Result RotateRefreshToken(string refreshTokenHash, DateTime expiresAtUtc)
        {
            if (string.IsNullOrWhiteSpace(refreshTokenHash))
            {
                return Result.Failure("Refresh token hash cannot be null or empty.");
            }
            if (expiresAtUtc <= DateTime.UtcNow)
                return Result.Failure("Refresh token expiry must be in the future.");

            RefreshTokenHash = refreshTokenHash;
            RevokeAt = null;
            RefreshTokenExpiresAtUtc = expiresAtUtc;

            return Result.Success();
        }

        public Result RevokeRefreshToken()
        {
            RefreshTokenHash = string.Empty;
            RefreshTokenExpiresAtUtc = null;
            RevokeAt = DateTime.UtcNow;
            return Result.Success();
        }

        public Result IssueEmailVerificationToken(string tokenHash, DateTime expiresAtUtc)
        {
            if (string.IsNullOrWhiteSpace(tokenHash) || expiresAtUtc <= DateTime.UtcNow)
                return Result.Failure("Email verification token is invalid.");

            EmailVerificationTokenHash = tokenHash;
            EmailVerificationTokenExpiresAtUtc = expiresAtUtc;
            return Result.Success();
        }

        public Result ConsumeEmailVerificationToken(string tokenHash, DateTime nowUtc)
        {
            if (string.IsNullOrWhiteSpace(tokenHash) ||
                !string.Equals(EmailVerificationTokenHash, tokenHash, StringComparison.Ordinal) ||
                !EmailVerificationTokenExpiresAtUtc.HasValue ||
                EmailVerificationTokenExpiresAtUtc.Value <= nowUtc)
                return Result.Failure("Email verification token is invalid or expired.");

            EmailVerificationTokenHash = string.Empty;
            EmailVerificationTokenExpiresAtUtc = null;
            return Result.Success();
        }

        public Result IssuePasswordResetToken(string tokenHash, DateTime expiresAtUtc)
        {
            if (string.IsNullOrWhiteSpace(tokenHash) || expiresAtUtc <= DateTime.UtcNow)
                return Result.Failure("Password reset token is invalid.");

            PasswordResetTokenHash = tokenHash;
            PasswordResetTokenExpiresAtUtc = expiresAtUtc;
            return Result.Success();
        }

        public Result ConsumePasswordResetToken(string tokenHash, DateTime nowUtc)
        {
            if (string.IsNullOrWhiteSpace(tokenHash) ||
                !string.Equals(PasswordResetTokenHash, tokenHash, StringComparison.Ordinal) ||
                !PasswordResetTokenExpiresAtUtc.HasValue ||
                PasswordResetTokenExpiresAtUtc.Value <= nowUtc)
                return Result.Failure("Password reset token is invalid or expired.");

            PasswordResetTokenHash = string.Empty;
            PasswordResetTokenExpiresAtUtc = null;
            return Result.Success();
        }

        public void ClearEmailVerificationToken()
        {
            EmailVerificationTokenHash = string.Empty;
            EmailVerificationTokenExpiresAtUtc = null;
        }
    }
}