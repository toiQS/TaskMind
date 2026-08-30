using Microsoft.EntityFrameworkCore;
using TaskMind.Domain.Commons.Cores;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Events;

namespace TaskMind.Domain.Entities
{
    /// <summary>Aggregate Root Certificate [MỚI] — chứng chỉ điện tử cấp cho User khi hoàn thành khoá học/bài kiểm tra đạt yêu cầu (mục 4.20).</summary>
    [Index(nameof(UserId))]
    [Index(nameof(VerificationCode), IsUnique = true)]
    public class Certificate : AggregateRoot
    {
        public Guid UserId { get; private set; }

        /// <summary>Liên kết tuỳ chọn tới Submission làm bằng chứng đánh giá.</summary>
        public Guid? SubmissionId { get; private set; }
        public string VerificationCode { get; private set; } = string.Empty;
        public DateTime IssuedAtUtc { get; private set; } = DateTime.UtcNow;

        private Certificate() { }

        private Certificate(Guid userId, Guid? submissionId)
        {
            UserId = userId;
            SubmissionId = submissionId;
            VerificationCode = GenerateVerificationCode();
        }

        public static Result<Certificate> Issue(Guid userId, Guid? submissionId = null)
        {
            if (userId == Guid.Empty)
                return Result<Certificate>.Failure("UserId không hợp lệ.");

            var certificate = new Certificate(userId, submissionId);

            certificate.AddDomainEvent(new CertificateIssuedEvent
            {
                CertificateId = certificate.Id,
                UserId = userId,
                VerificationCode = certificate.VerificationCode
            });

            return Result<Certificate>.Success(certificate);
        }

        private static string GenerateVerificationCode()
            => Guid.NewGuid().ToString("N")[..12].ToUpperInvariant();
    }
}
