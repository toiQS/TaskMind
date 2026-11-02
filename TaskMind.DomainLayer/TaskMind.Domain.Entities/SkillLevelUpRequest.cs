using Microsoft.EntityFrameworkCore;
using TaskMind.DomainLayer.TaskMind.Domain.Commons.Cores;
using TaskMind.DomainLayer.TaskMind.Domain.Commons.Result;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;
using TaskMind.DomainLayer.TaskMind.Domain.Events;

namespace TaskMind.DomainLayer.TaskMind.Domain.Entities
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
            if (!Enum.IsDefined(currentLevel))
                return Result<SkillLevelUpRequest>.Failure("Level kỹ năng hiện tại không hợp lệ.");
            if (!Enum.IsDefined(requestType))
                return Result<SkillLevelUpRequest>.Failure("Hình thức nâng cấp không hợp lệ.");
            if (requestType == SkillLevelUpMethod.Endorsement && approverAccountId == Guid.Empty)
                return Result<SkillLevelUpRequest>.Failure("Yêu cầu bảo lãnh (Endorsement) cần xác định ApproverAccountId.");

            return Result<SkillLevelUpRequest>.Success(new SkillLevelUpRequest(userId, skillId, currentLevel, approverAccountId, requestType));
        }

        /// <summary>Gắn Submission làm bằng chứng đánh giá (chỉ áp dụng cho RequestType = Assessment - mục 4.3.1). [MỚI]</summary>
        public Result LinkSubmission(Submission submission)
        {
            if (RequestType != SkillLevelUpMethod.Assessment)
                return Result.Failure("Chỉ yêu cầu theo hình thức Assessment mới có thể liên kết Submission.");
            if (RequestStatus != SkillLevelUpRequestStatus.PendingAssessment)
                return Result.Failure("Yêu cầu không còn chờ đánh giá.");
            if (SubmissionId.HasValue)
                return Result.Failure("Yêu cầu đã được liên kết với một Submission.");
            if (submission is null || submission.Id == Guid.Empty)
                return Result.Failure("Submission không hợp lệ.");
            if (submission.UserId != UserId)
                return Result.Failure("Submission không thuộc người yêu cầu.");
            if (!submission.SubmittedAtUtc.HasValue || submission.IsGraded)
                return Result.Failure("Submission phải đã nộp và chưa được chấm.");

            SubmissionId = submission.Id;
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
        public Result Approve(Guid approverAccountId)
        {
            if (RequestType != SkillLevelUpMethod.Endorsement ||
                RequestStatus != SkillLevelUpRequestStatus.PendingEndorsement)
                return Result.Failure("Chỉ yêu cầu Endorsement đang chờ mới có thể được duyệt trực tiếp.");
            if (approverAccountId == Guid.Empty || approverAccountId != ApproverAccountId)
                return Result.Failure("Tài khoản duyệt không khớp người bảo lãnh được chỉ định.");

            RequestStatus = SkillLevelUpRequestStatus.Approved;
            return Result.Success();
        }

        public Result ApplyAssessmentResult(Submission submission)
        {
            if (RequestType != SkillLevelUpMethod.Assessment ||
                RequestStatus != SkillLevelUpRequestStatus.PendingAssessment)
                return Result.Failure("Chỉ yêu cầu Assessment đang chờ mới nhận kết quả bài kiểm tra.");
            if (submission is null || !SubmissionId.HasValue || submission.Id != SubmissionId.Value)
                return Result.Failure("Submission không khớp yêu cầu nâng cấp.");
            if (submission.UserId != UserId || !submission.IsGraded)
                return Result.Failure("Submission phải thuộc người yêu cầu và đã được chấm.");

            if (submission.Score >= 5m)
            {
                RequestStatus = SkillLevelUpRequestStatus.Approved;
                return Result.Success();
            }

            RejectionReason = "Không đạt bài kiểm tra đánh giá kỹ năng.";
            RequestStatus = SkillLevelUpRequestStatus.Rejected;
            AddDomainEvent(new SkillLevelUpRejectedEvent
            {
                RequestId = Id,
                UserId = UserId,
                SkillId = SkillId,
                RequestType = RequestType,
                Reason = RejectionReason
            });
            return Result.Success();
        }

        /// <summary>Từ chối yêu cầu Endorsement do người bảo lãnh được chỉ định.</summary>
        public Result Reject(Guid approverAccountId, string reason)
        {
            if (RequestType != SkillLevelUpMethod.Endorsement ||
                RequestStatus != SkillLevelUpRequestStatus.PendingEndorsement)
                return Result.Failure("Chỉ yêu cầu Endorsement đang chờ mới có thể bị từ chối trực tiếp.");
            if (approverAccountId == Guid.Empty || approverAccountId != ApproverAccountId)
                return Result.Failure("Tài khoản từ chối không khớp người bảo lãnh được chỉ định.");
            if (string.IsNullOrWhiteSpace(reason))
                return Result.Failure("Phải nêu lý do từ chối yêu cầu.");

            RejectionReason = reason.Trim();
            RequestStatus = SkillLevelUpRequestStatus.Rejected;
            AddDomainEvent(new SkillLevelUpRejectedEvent
            {
                RequestId = Id,
                UserId = UserId,
                SkillId = SkillId,
                RequestType = RequestType,
                Reason = RejectionReason
            });
            return Result.Success();
        }
    }
}