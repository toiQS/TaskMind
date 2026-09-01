using Microsoft.EntityFrameworkCore;
using TaskMind.Domain.Commons.Cores;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Enums;

namespace TaskMind.Domain.Entities
{
    /// <summary>
    /// Aggregate Root SkillHistoryEntry [MỚI - v2.1] — nhật ký bất biến (append-only) của mọi thay đổi
    /// kỹ năng một User, dù xuất phát từ luồng User tự khởi xướng (mục 4.3.1) hay công ty chủ động
    /// phản ánh (mục 4.3.2), BAO GỒM cả các đề xuất bị từ chối/không thành công (mục 4.3.3). Mục đích:
    /// đảm bảo tính minh bạch và có thể truy vết toàn bộ quá trình hình thành hồ sơ năng lực của một
    /// cá nhân qua nhiều tổ chức khác nhau, phục vụ tra cứu khi có tranh chấp hoặc xác minh cho bên
    /// thứ ba (ví dụ nhà tuyển dụng mới).
    /// </summary>
    [Index(nameof(UserId), nameof(SkillId))]
    [Index(nameof(CompanyId))]
    public class SkillHistoryEntry : AggregateRoot
    {
        public Guid UserId { get; private set; }
        public Guid SkillId { get; private set; }
        public SkillChangeSource ChangeSource { get; private set; }

        public SkillLevel? LevelBefore { get; private set; }
        public SkillLevel? LevelAfter { get; private set; }
        public bool IsNewlyAdded { get; private set; }

        /// <summary>Tài khoản đứng tên chịu trách nhiệm (User tự yêu cầu, Approver, Project Manager, hoặc Admin company/Admin hệ thống).</summary>
        public Guid ResponsibleAccountId { get; private set; }

        /// <summary>Công ty liên quan tại thời điểm đó — null nếu ChangeSource = UserInitiated không gắn công ty cụ thể.</summary>
        public Guid? CompanyId { get; private set; }
        public Guid? ProjectId { get; private set; }

        /// <summary>Khoảng thời gian nhân sự thực sự thuộc công ty đó, theo vòng đời tài khoản liên kết (mục 2.1.1).</summary>
        public DateTimeOffset? TenureStartUtc { get; private set; }
        public DateTimeOffset? TenureEndUtc { get; private set; }

        /// <summary>Mô tả/liên kết tham chiếu bằng chứng và kết quả bài kiểm tra xác minh liên quan (nếu có).</summary>
        public string EvidenceDescription { get; private set; } = string.Empty;
        public Guid? RelatedSubmissionId { get; private set; }

        /// <summary>SkillLevelUpRequest.Id (mục 4.3.1) hoặc CompanySkillReflectionRequest.Id (mục 4.3.2) đã dẫn tới mục lịch sử này.</summary>
        public Guid? RelatedRequestId { get; private set; }

        public SkillHistoryOutcome Outcome { get; private set; }
        public string? RejectionReason { get; private set; }

        public DateTimeOffset CreatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;

        private SkillHistoryEntry() { }

        private SkillHistoryEntry(
            Guid userId, Guid skillId, SkillChangeSource changeSource,
            SkillLevel? levelBefore, SkillLevel? levelAfter, bool isNewlyAdded,
            Guid responsibleAccountId, Guid? companyId, Guid? projectId,
            DateTimeOffset? tenureStartUtc, DateTimeOffset? tenureEndUtc,
            string evidenceDescription, Guid? relatedSubmissionId, Guid? relatedRequestId,
            SkillHistoryOutcome outcome, string? rejectionReason)
        {
            UserId = userId;
            SkillId = skillId;
            ChangeSource = changeSource;
            LevelBefore = levelBefore;
            LevelAfter = levelAfter;
            IsNewlyAdded = isNewlyAdded;
            ResponsibleAccountId = responsibleAccountId;
            CompanyId = companyId;
            ProjectId = projectId;
            TenureStartUtc = tenureStartUtc;
            TenureEndUtc = tenureEndUtc;
            EvidenceDescription = evidenceDescription;
            RelatedSubmissionId = relatedSubmissionId;
            RelatedRequestId = relatedRequestId;
            Outcome = outcome;
            RejectionReason = rejectionReason;
        }

        /// <summary>Ghi nhận một mục lịch sử kỹ năng — dùng cho cả trường hợp đã áp dụng lẫn bị từ chối (mục 4.3.3).</summary>
        public static Result<SkillHistoryEntry> Record(
            Guid userId, Guid skillId, SkillChangeSource changeSource,
            Guid responsibleAccountId, SkillHistoryOutcome outcome,
            SkillLevel? levelBefore = null, SkillLevel? levelAfter = null, bool isNewlyAdded = false,
            Guid? companyId = null, Guid? projectId = null,
            DateTimeOffset? tenureStartUtc = null, DateTimeOffset? tenureEndUtc = null,
            string? evidenceDescription = null, Guid? relatedSubmissionId = null, Guid? relatedRequestId = null,
            string? rejectionReason = null)
        {
            if (userId == Guid.Empty || skillId == Guid.Empty)
                return Result<SkillHistoryEntry>.Failure("UserId/SkillId không hợp lệ.");
            if (responsibleAccountId == Guid.Empty)
                return Result<SkillHistoryEntry>.Failure("Phải xác định người/đơn vị đứng tên chịu trách nhiệm.");
            if (outcome == SkillHistoryOutcome.Rejected && string.IsNullOrWhiteSpace(rejectionReason))
                return Result<SkillHistoryEntry>.Failure("Phải nêu lý do khi ghi nhận một đề xuất bị từ chối.");

            var entry = new SkillHistoryEntry(
                userId, skillId, changeSource, levelBefore, levelAfter, isNewlyAdded,
                responsibleAccountId, companyId, projectId, tenureStartUtc, tenureEndUtc,
                evidenceDescription?.Trim() ?? string.Empty, relatedSubmissionId, relatedRequestId,
                outcome, rejectionReason?.Trim());

            return Result<SkillHistoryEntry>.Success(entry);
        }
    }
}
