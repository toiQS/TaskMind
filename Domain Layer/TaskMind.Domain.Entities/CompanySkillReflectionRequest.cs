using Microsoft.EntityFrameworkCore;
using TaskMind.Domain.Commons.Cores;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Domain.Entities
{
    /// <summary>
    /// Aggregate Root CompanySkillReflectionRequest [MỚI - v2.1] — đề xuất công ty chủ động phản ánh
    /// kỹ năng nhân sự (nâng cấp/hạ cấp/bổ sung mới), mục 4.3.2. Khác với SkillLevelUpRequest (User tự
    /// khởi xướng, mục 4.3.1), aggregate này do công ty (qua Project Manager của dự án liên quan, hoặc
    /// Admin company nếu chính nhân sự là PM/dự án không còn người phụ trách) khởi tạo, và LUÔN cần
    /// xác minh khách quan qua bài kiểm tra hệ thống (TestPaper/Submission) trước khi áp dụng chính
    /// thức lên SkillProfile của User — công ty KHÔNG được tự ý sửa trực tiếp vì SkillProfile là dữ
    /// liệu toàn cục dùng chung cho mọi hoạt động của User (kể cả sau khi rời công ty).
    /// </summary>
    [Index(nameof(CompanyId), nameof(Status))]
    [Index(nameof(UserId), nameof(SkillId))]
    public class CompanySkillReflectionRequest : AggregateRoot
    {
        public Guid CompanyId { get; private set; }

        /// <summary>User gốc bị ảnh hưởng — SkillProfile gắn với User, không gắn với từng bản ghi Staff riêng lẻ.</summary>
        public Guid UserId { get; private set; }

        /// <summary>Bản ghi Staff cụ thể tại thời điểm đề xuất, dùng để xác định chính xác khoảng thời gian công tác (mục 2.1.1, 4.3.3).</summary>
        public Guid StaffAccountId { get; private set; }
        public Guid SkillId { get; private set; }
        public SkillReflectionType ReflectionType { get; private set; }

        /// <summary>Level đề xuất — bắt buộc với Up/Add; null với Down (level mới chỉ xác định sau khi xác minh không đạt).</summary>
        public SkillLevel? ProposedLevel { get; private set; }

        /// <summary>Level hiện tại ghi nhận tại thời điểm đề xuất — bắt buộc với Down.</summary>
        public SkillLevel? CurrentLevelAtRequest { get; private set; }

        /// <summary>Dự án làm căn cứ (công cụ/thư viện đã dùng, hoặc sự việc phát sinh) — tuỳ chọn.</summary>
        public Guid? ProjectId { get; private set; }

        /// <summary>
        /// Mô tả/liên kết tham chiếu bằng chứng. (mục 8 - vấn đề mở: hiện chỉ lưu mô tả/link tới nguồn
        /// lưu trữ bên ngoài, TaskMind không trực tiếp lưu trữ file media.)
        /// </summary>
        public string EvidenceDescription { get; private set; } = string.Empty;

        /// <summary>Tần suất phát sinh vấn đề — chỉ có ý nghĩa với Down.</summary>
        public int? IncidentFrequency { get; private set; }

        /// <summary>Người đứng tên chịu trách nhiệm đề xuất: PM của dự án liên quan, hoặc Admin company (mục 4.3.2).</summary>
        public Guid ResponsibleAccountId { get; private set; }

        public SkillReflectionStatus Status { get; private set; }
        public Guid? AssignedTestPaperId { get; private set; }
        public Guid? VerificationSubmissionId { get; private set; }

        /// <summary>Level thực tế được áp dụng sau xác minh (Up/Add: = ProposedLevel; Down khi không đạt: = CurrentLevelAtRequest - 1).</summary>
        public SkillLevel? ResultLevel { get; private set; }
        public string? RejectionReason { get; private set; }

        public DateTimeOffset CreatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? ResolvedAtUtc { get; private set; }

        private CompanySkillReflectionRequest() { }

        private CompanySkillReflectionRequest(
            Guid companyId, Guid userId, Guid staffAccountId, Guid skillId,
            SkillReflectionType reflectionType, Guid responsibleAccountId,
            Guid? projectId, string evidenceDescription,
            SkillLevel? proposedLevel, SkillLevel? currentLevelAtRequest, int? incidentFrequency)
        {
            CompanyId = companyId;
            UserId = userId;
            StaffAccountId = staffAccountId;
            SkillId = skillId;
            ReflectionType = reflectionType;
            ResponsibleAccountId = responsibleAccountId;
            ProjectId = projectId;
            EvidenceDescription = evidenceDescription;
            ProposedLevel = proposedLevel;
            CurrentLevelAtRequest = currentLevelAtRequest;
            IncidentFrequency = incidentFrequency;

            // Down cần Admin hệ thống xem xét trước khi tổ chức xác minh lại; Up/Add vào thẳng hàng chờ xác minh (mục 4.3.2).
            Status = reflectionType == SkillReflectionType.Down
                ? SkillReflectionStatus.PendingAdminReview
                : SkillReflectionStatus.PendingVerification;
        }

        private static Result<CompanySkillReflectionRequest> Create(
            SkillReflectionType type,
            Guid companyId, Guid userId, Guid staffAccountId, Guid skillId,
            Guid responsibleAccountId, Guid? projectId, string evidenceDescription,
            SkillLevel? proposedLevel, SkillLevel? currentLevelAtRequest, int? incidentFrequency)
        {
            if (companyId == Guid.Empty || userId == Guid.Empty || staffAccountId == Guid.Empty || skillId == Guid.Empty)
                return Result<CompanySkillReflectionRequest>.Failure("Thông tin đề xuất phản ánh kỹ năng không hợp lệ.");
            if (responsibleAccountId == Guid.Empty)
                return Result<CompanySkillReflectionRequest>.Failure("Phải xác định người đứng tên chịu trách nhiệm đề xuất (mục 4.3.2).");
            if (string.IsNullOrWhiteSpace(evidenceDescription))
                return Result<CompanySkillReflectionRequest>.Failure("Phải kèm theo mô tả/bằng chứng cho đề xuất.");

            if (type is SkillReflectionType.Up or SkillReflectionType.Add && proposedLevel is null)
                return Result<CompanySkillReflectionRequest>.Failure("Phải xác định level đề xuất đối với Up/Add.");
            if (type == SkillReflectionType.Down && currentLevelAtRequest is null)
                return Result<CompanySkillReflectionRequest>.Failure("Phải xác định level hiện tại làm căn cứ đối với Down.");

            var request = new CompanySkillReflectionRequest(
                companyId, userId, staffAccountId, skillId, type, responsibleAccountId,
                projectId, evidenceDescription.Trim(), proposedLevel, currentLevelAtRequest, incidentFrequency);

            request.AddDomainEvent(new CompanySkillReflectionRequestedEvent
            {
                RequestId = request.Id,
                CompanyId = companyId,
                UserId = userId,
                StaffAccountId = staffAccountId,
                SkillId = skillId,
                ReflectionType = type,
                ResponsibleAccountId = responsibleAccountId,
                RequiresAdminReview = type == SkillReflectionType.Down
            });

            return Result<CompanySkillReflectionRequest>.Success(request);
        }

        /// <summary>Đề xuất nâng cấp độ kỹ năng (mục 4.3.2 - Up), kèm bằng chứng công cụ/thư viện/framework đã dùng trong dự án.</summary>
        public static Result<CompanySkillReflectionRequest> CreateUp(
            Guid companyId, Guid userId, Guid staffAccountId, Guid skillId, Guid responsibleAccountId,
            Guid? projectId, string evidenceDescription, SkillLevel proposedLevel)
            => Create(SkillReflectionType.Up, companyId, userId, staffAccountId, skillId, responsibleAccountId,
                projectId, evidenceDescription, proposedLevel, null, null);

        /// <summary>Đề xuất bổ sung kỹ năng mới chưa có trong SkillProfile (mục 4.3.2 - Add), công ty được tự chọn level khởi điểm.</summary>
        public static Result<CompanySkillReflectionRequest> CreateAdd(
            Guid companyId, Guid userId, Guid staffAccountId, Guid skillId, Guid responsibleAccountId,
            Guid? projectId, string evidenceDescription, SkillLevel proposedLevel)
            => Create(SkillReflectionType.Add, companyId, userId, staffAccountId, skillId, responsibleAccountId,
                projectId, evidenceDescription, proposedLevel, null, null);

        /// <summary>Đề xuất hạ cấp độ kỹ năng (mục 4.3.2 - Down), kèm bằng chứng sai phạm/không tương xứng và tần suất phát sinh.</summary>
        public static Result<CompanySkillReflectionRequest> CreateDown(
            Guid companyId, Guid userId, Guid staffAccountId, Guid skillId, Guid responsibleAccountId,
            Guid? projectId, string evidenceDescription, SkillLevel currentLevel, int? incidentFrequency = null)
            => Create(SkillReflectionType.Down, companyId, userId, staffAccountId, skillId, responsibleAccountId,
                projectId, evidenceDescription, null, currentLevel, incidentFrequency);

        /// <summary>Admin hệ thống chấp nhận xử lý đề xuất Down, chuyển sang chờ nhân sự làm lại bài kiểm tra (mục 4.3.2).</summary>
        public Result AdminAccept(Guid approverAdminId)
        {
            if (ReflectionType != SkillReflectionType.Down)
                return Result.Failure("Chỉ đề xuất hạ cấp (Down) mới cần Admin xem xét trước.");
            if (Status != SkillReflectionStatus.PendingAdminReview)
                return Result.Failure("Đề xuất không ở trạng thái chờ Admin xem xét.");
            if (approverAdminId == Guid.Empty)
                return Result.Failure("ApproverAdminId không hợp lệ.");

            Status = SkillReflectionStatus.PendingVerification;

            AddDomainEvent(new SkillReflectionAdminAcceptedEvent
            {
                RequestId = Id,
                UserId = UserId,
                SkillId = SkillId,
                ApproverAdminId = approverAdminId
            });

            return Result.Success();
        }

        /// <summary>Admin hệ thống từ chối xử lý ngay từ đầu (bằng chứng chưa thoả đáng) — đề xuất kết thúc, không cần retest.</summary>
        public Result AdminDismiss(string reason)
        {
            if (ReflectionType != SkillReflectionType.Down)
                return Result.Failure("Chỉ đề xuất hạ cấp (Down) mới cần Admin xem xét trước.");
            if (Status != SkillReflectionStatus.PendingAdminReview)
                return Result.Failure("Đề xuất không ở trạng thái chờ Admin xem xét.");

            Status = SkillReflectionStatus.Rejected;
            RejectionReason = string.IsNullOrWhiteSpace(reason) ? "Admin hệ thống từ chối xử lý đề xuất." : reason.Trim();
            ResolvedAtUtc = DateTimeOffset.UtcNow;

            AddDomainEvent(new SkillReflectionRejectedEvent
            {
                RequestId = Id,
                UserId = UserId,
                SkillId = SkillId,
                ReflectionType = ReflectionType,
                Reason = RejectionReason
            });

            return Result.Success();
        }

        /// <summary>Gán bài kiểm tra xác minh đúng phạm vi/level được đề xuất (mục 4.3.2).</summary>
        public Result AssignTestPaper(Guid testPaperId)
        {
            if (Status != SkillReflectionStatus.PendingVerification)
                return Result.Failure("Chỉ có thể gán bài kiểm tra khi đề xuất đang chờ xác minh.");
            if (testPaperId == Guid.Empty)
                return Result.Failure("TestPaperId không hợp lệ.");

            AssignedTestPaperId = testPaperId;
            return Result.Success();
        }

        /// <summary>Liên kết bài làm (Submission) làm bằng chứng kết quả xác minh.</summary>
        public Result LinkVerificationSubmission(Guid submissionId)
        {
            if (submissionId == Guid.Empty)
                return Result.Failure("SubmissionId không hợp lệ.");

            VerificationSubmissionId = submissionId;
            return Result.Success();
        }

        /// <summary>
        /// Xử lý kết quả xác minh (mục 4.3.2). Với Up/Add: đạt bài kiểm tra mới được áp dụng level đề
        /// xuất, không đạt thì bị từ chối. Với Down: ĐẠT nghĩa là giữ nguyên (đề xuất không thành công),
        /// KHÔNG ĐẠT mới chính thức hạ cấp — tránh trường hợp một phía đơn phương kết luận mà không qua
        /// xác minh khách quan.
        /// </summary>
        public Result ApplyVerificationResult(bool testPassed)
        {
            if (Status != SkillReflectionStatus.PendingVerification)
                return Result.Failure("Đề xuất không ở trạng thái chờ xác minh.");

            ResolvedAtUtc = DateTimeOffset.UtcNow;

            if (ReflectionType == SkillReflectionType.Down)
            {
                if (testPassed)
                {
                    Status = SkillReflectionStatus.Rejected;
                    RejectionReason = "Nhân sự đạt bài kiểm tra xác minh lại — giữ nguyên cấp độ hiện tại, đề xuất không thành công.";

                    AddDomainEvent(new SkillReflectionRejectedEvent
                    {
                        RequestId = Id,
                        UserId = UserId,
                        SkillId = SkillId,
                        ReflectionType = ReflectionType,
                        Reason = RejectionReason
                    });
                }
                else
                {
                    var downgraded = Math.Max((int)SkillLevel.Beginner, (int)CurrentLevelAtRequest!.Value - 1);
                    ResultLevel = (SkillLevel)downgraded;
                    Status = SkillReflectionStatus.Applied;

                    AddDomainEvent(new SkillReflectionAppliedEvent
                    {
                        RequestId = Id,
                        UserId = UserId,
                        SkillId = SkillId,
                        NewLevel = ResultLevel.Value,
                        IsNewSkill = false,
                        ReflectionType = ReflectionType
                    });
                }
            }
            else
            {
                if (testPassed)
                {
                    ResultLevel = ProposedLevel;
                    Status = SkillReflectionStatus.Applied;

                    AddDomainEvent(new SkillReflectionAppliedEvent
                    {
                        RequestId = Id,
                        UserId = UserId,
                        SkillId = SkillId,
                        NewLevel = ResultLevel!.Value,
                        IsNewSkill = ReflectionType == SkillReflectionType.Add,
                        ReflectionType = ReflectionType
                    });
                }
                else
                {
                    Status = SkillReflectionStatus.Rejected;
                    RejectionReason = "Không đạt bài kiểm tra xác minh tương ứng với level/kỹ năng đề xuất.";

                    AddDomainEvent(new SkillReflectionRejectedEvent
                    {
                        RequestId = Id,
                        UserId = UserId,
                        SkillId = SkillId,
                        ReflectionType = ReflectionType,
                        Reason = RejectionReason
                    });
                }
            }

            return Result.Success();
        }
    }
}
