using Microsoft.EntityFrameworkCore;
using TaskMind.Domain.Commons.Cores;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Domain.Entities
{
    /// <summary>
    /// Yêu cầu nâng level kỹ năng (mục 4.3.1). [CẬP NHẬT] bổ sung RequestType (SkillLevelUpMethod:
    /// Endorsement/Assessment) và SubmissionId (liên kết tuỳ chọn tới Submission làm bằng chứng khi
    /// RequestType = Assessment) theo tài liệu v2.
    /// - Endorsement: cần bảo lãnh/xác nhận từ người có thẩm quyền cao hơn trong đơn vị công tác của
    ///   User (Technical leader, Admin company, Admin school).
    /// - Assessment: User trải qua chu trình đánh giá năng lực chuẩn (TestPaper/Submission).
    /// </summary>
    [Index(nameof(UserId), nameof(SkillId), nameof(RequestStatus))]
    public class SkillLevelUpRequest : AggregateRoot
    {
        public Guid UserId { get; private set; }
        public Guid SkillId { get; private set; }
        public SkillLevel CurrentLevel { get; private set; }
        public Guid ApproverAccountId { get; private set; }

        /// <summary>Phương thức nâng cấp: Endorsement hay Assessment (mục 4.3.1). [MỚI]</summary>
        public SkillLevelUpMethod RequestType { get; private set; } = SkillLevelUpMethod.Endorsement;

        /// <summary>Liên kết tuỳ chọn tới Submission làm bằng chứng, chỉ áp dụng khi RequestType = Assessment. [MỚI]</summary>
        public Guid? SubmissionId { get; private set; }

        /// <summary>Đặt tên khác "Status" để không che khuất EntityBase.Status (EntityStatus).</summary>
        public SkillLevelUpRequestStatus RequestStatus { get; private set; } = SkillLevelUpRequestStatus.PendingEndorsement;
        public string? RejectionReason { get; private set; }

        private SkillLevelUpRequest() { }

        private SkillLevelUpRequest(Guid userId, Guid skillId, SkillLevel currentLevel, Guid approverAccountId, SkillLevelUpMethod requestType)
        {
            UserId = userId;
            SkillId = skillId;
            CurrentLevel = currentLevel;
            ApproverAccountId = approverAccountId;
            RequestType = requestType;
            RequestStatus = requestType == SkillLevelUpMethod.Assessment
                ? SkillLevelUpRequestStatus.PendingAssessment
                : SkillLevelUpRequestStatus.PendingEndorsement;
        }

        public static Result<SkillLevelUpRequest> Create(
            Guid userId,
            Guid skillId,
            SkillLevel currentLevel,
            Guid approverAccountId,
            SkillLevelUpMethod requestType = SkillLevelUpMethod.Endorsement)
        {
            if (userId == Guid.Empty || skillId == Guid.Empty)
                return Result<SkillLevelUpRequest>.Failure("Thông tin yêu cầu nâng level không hợp lệ.");
            if (requestType == SkillLevelUpMethod.Endorsement && approverAccountId == Guid.Empty)
                return Result<SkillLevelUpRequest>.Failure("Yêu cầu bảo lãnh (Endorsement) cần xác định ApproverAccountId.");

            return Result<SkillLevelUpRequest>.Success(new SkillLevelUpRequest(userId, skillId, currentLevel, approverAccountId, requestType));
        }

        /// <summary>Gắn Submission làm bằng chứng đánh giá (chỉ áp dụng cho RequestType = Assessment - mục 4.3.1). [MỚI]</summary>
        public Result LinkSubmission(Guid submissionId)
        {
            if (RequestType != SkillLevelUpMethod.Assessment)
                return Result.Failure("Chỉ yêu cầu theo hình thức Assessment mới có thể liên kết Submission.");
            if (submissionId == Guid.Empty)
                return Result.Failure("SubmissionId không hợp lệ.");

            SubmissionId = submissionId;
            return Result.Success();
        }

        /// <summary>
        /// [CẬP NHẬT - fix] KHÔNG còn tự phát sinh SkillLevelApprovedEvent tại đây. Trước đây cả
        /// Approve() (trên aggregate này) LẪN SkillProfile.ApplyLevelUp() đều raise cùng một
        /// SkillLevelApprovedEvent cho cùng một lần duyệt — khi cả hai aggregate được track trong
        /// cùng một SaveChangesAsync, vòng lặp publish domain event sẽ gom và bắn sự kiện này 2 LẦN,
        /// khiến SkillLevelApprovedEventHandler chạy 2 lần → user nhận trùng Notification + email.
        /// Nguồn phát sự kiện duy nhất giờ là SkillProfile.ApplyLevelUp(), vì đó mới là nơi level thực
        /// sự thay đổi trên hồ sơ. Approve() ở đây chỉ còn đổi trạng thái của chính request.
        /// </summary>
        public Result Approve()
        {
            if (RequestStatus is SkillLevelUpRequestStatus.Approved or SkillLevelUpRequestStatus.Rejected)
                return Result.Failure("Yêu cầu đã được xử lý trước đó.");

            RequestStatus = SkillLevelUpRequestStatus.Approved;
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