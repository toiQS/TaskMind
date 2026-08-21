using Microsoft.EntityFrameworkCore;
using TaskMind.Domain.Commons.Cores;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Domain.Entities
{
    /// <summary>
    /// Yêu cầu nâng level kỹ năng (mục 4.3.1): cần endorsement từ người có thẩm quyền cao hơn
    /// trong đơn vị công tác của User (Technical leader, Admin company, Admin school), hoặc trải
    /// qua chu trình đánh giá năng lực chuẩn của hệ thống (Assessment context).
    /// </summary>
    [Index(nameof(UserId), nameof(SkillId), nameof(RequestStatus))]
    public class SkillLevelUpRequest : AggregateRoot
    {
        public Guid UserId { get; private set; }
        public Guid SkillId { get; private set; }
        public SkillLevel CurrentLevel { get; private set; }
        public Guid ApproverAccountId { get; private set; }

        /// <summary>Đặt tên khác "Status" để không che khuất EntityBase.Status (EntityStatus).</summary>
        public SkillLevelUpRequestStatus RequestStatus { get; private set; } = SkillLevelUpRequestStatus.PendingEndorsement;
        public string? RejectionReason { get; private set; }

        private SkillLevelUpRequest() { }

        private SkillLevelUpRequest(Guid userId, Guid skillId, SkillLevel currentLevel, Guid approverAccountId)
        {
            UserId = userId;
            SkillId = skillId;
            CurrentLevel = currentLevel;
            ApproverAccountId = approverAccountId;
        }

        public static Result<SkillLevelUpRequest> Create(Guid userId, Guid skillId, SkillLevel currentLevel, Guid approverAccountId)
        {
            if (userId == Guid.Empty || skillId == Guid.Empty || approverAccountId == Guid.Empty)
                return Result<SkillLevelUpRequest>.Failure("Thông tin yêu cầu nâng level không hợp lệ.");

            return Result<SkillLevelUpRequest>.Success(new SkillLevelUpRequest(userId, skillId, currentLevel, approverAccountId));
        }

        public Result Approve()
        {
            if (RequestStatus is SkillLevelUpRequestStatus.Approved or SkillLevelUpRequestStatus.Rejected)
                return Result.Failure("Yêu cầu đã được xử lý trước đó.");

            RequestStatus = SkillLevelUpRequestStatus.Approved;
            AddDomainEvent(new SkillLevelApprovedEvent
            {
                UserId = UserId,
                SkillId = SkillId,
                NewLevel = (SkillLevel)Math.Min((int)SkillLevel.Expert, (int)CurrentLevel + 1)
            });
            return Result.Success();
        }

        /// <summary>Từ chối do không có kinh nghiệm thực tế/không đạt yêu cầu xác minh — kích hoạt hạ level x2 (mục 4.3.1).</summary>
        public Result Reject(string reason)
        {
            if (RequestStatus is SkillLevelUpRequestStatus.Approved or SkillLevelUpRequestStatus.Rejected)
                return Result.Failure("Yêu cầu đã được xử lý trước đó.");

            RequestStatus = SkillLevelUpRequestStatus.Rejected;
            RejectionReason = reason;

            AddDomainEvent(new SkillLevelUpRejectedEvent
            {
                UserId = UserId,
                SkillId = SkillId,
                Reason = reason
            });
            return Result.Success();
        }
    }
}
