using Microsoft.EntityFrameworkCore;
using TaskMind.DomainLayer.TaskMind.Domain.Commons.Cores;
using TaskMind.DomainLayer.TaskMind.Domain.Commons.Result;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;
using TaskMind.DomainLayer.TaskMind.Domain.Events;

namespace TaskMind.DomainLayer.TaskMind.Domain.Entities
{
    /// <summary>
    /// Aggregate Root SkillProfile — hồ sơ kỹ năng cá nhân của một User (mục 4.3),
    /// khác với SkillCatalog (mục 4.15, danh mục chuẩn hoá toàn hệ thống).
    /// </summary>
    [Index(nameof(UserId), IsUnique = true)]
    public class SkillProfile : AggregateRoot
    {
        public Guid UserId { get; private set; }

        private readonly List<UserSkillRecord> _records = new();
        public IReadOnlyCollection<UserSkillRecord> Records => _records.AsReadOnly();

        private SkillProfile() { }

        private SkillProfile(Guid userId) { UserId = userId; }

        public static Result<SkillProfile> Create(Guid userId)
        {
            if (userId == Guid.Empty) return Result<SkillProfile>.Failure("UserId không hợp lệ.");
            return Result<SkillProfile>.Success(new SkillProfile(userId));
        }

        public Result DeclareSkill(Guid skillId, SkillLevel level)
        {
            if (skillId == Guid.Empty) return Result.Failure("SkillId không hợp lệ.");
            if (!Enum.IsDefined(level)) return Result.Failure("Level kỹ năng không hợp lệ.");
            if (_records.Any(r => r.SkillId == skillId))
                return Result.Failure("Kỹ năng đã tồn tại trong hồ sơ, dùng RequestLevelUp để nâng level.");

            _records.Add(UserSkillRecord.Create(skillId, level));
            return Result.Success();
        }

        public Result Endorse(Guid skillId, Guid endorserId)
        {
            if (endorserId == Guid.Empty) return Result.Failure("EndorserId không hợp lệ.");
            var record = _records.FirstOrDefault(r => r.SkillId == skillId);
            if (record == null) return Result.Failure("Không tìm thấy kỹ năng trong hồ sơ.");
            return record.AddEndorsement(endorserId);
        }

        /// <summary>
        /// [CẬP NHẬT] Khởi tạo yêu cầu nâng level kỹ năng (mục 4.3.1) với requestType tường minh
        /// (Endorsement/Assessment) theo tài liệu v2 — trước đây chỉ cần approverAccountId mặc định
        /// theo hướng Endorsement.
        /// </summary>
        public Result<SkillLevelUpRequest> RequestLevelUp(Guid skillId, Guid approverAccountId, SkillLevelUpMethod requestType = SkillLevelUpMethod.Endorsement)
        {
            var record = _records.FirstOrDefault(r => r.SkillId == skillId);
            if (record == null) return Result<SkillLevelUpRequest>.Failure("Không tìm thấy kỹ năng trong hồ sơ.");
            if (record.Level == SkillLevel.Expert) return Result<SkillLevelUpRequest>.Failure("Kỹ năng đã đạt cấp cao nhất.");

            var requestResult = SkillLevelUpRequest.Create(UserId, skillId, record.Level, approverAccountId, requestType);
            if (!requestResult.IsSuccess) return requestResult;

            AddDomainEvent(new SkillLevelUpRequestedEvent
            {
                UserId = UserId,
                SkillId = skillId,
                CurrentLevel = record.Level,
                RequestId = requestResult.Data!.Id,
                RequestType = requestType
            });

            return requestResult;
        }

        /// <summary>Áp dụng nâng level sau khi request được duyệt.</summary>
        public Result ApplyLevelUp(SkillLevelUpRequest request)
        {
            if (request is null || request.RequestStatus != SkillLevelUpRequestStatus.Approved)
                return Result.Failure("Chỉ yêu cầu nâng cấp đã được duyệt mới có thể áp dụng.");
            if (request.UserId != UserId)
                return Result.Failure("Yêu cầu nâng cấp không thuộc hồ sơ này.");

            var record = _records.FirstOrDefault(r => r.SkillId == request.SkillId);
            if (record == null) return Result.Failure("Không tìm thấy kỹ năng trong hồ sơ.");
            if (record.Level != request.CurrentLevel)
                return Result.Failure("Level hiện tại đã thay đổi kể từ khi tạo yêu cầu.");
            if (record.Level == SkillLevel.Expert)
                return Result.Failure("Kỹ năng đã đạt cấp cao nhất.");

            var newLevel = (SkillLevel)((int)record.Level + 1);
            record.SetLevel(newLevel);
            AddDomainEvent(new SkillLevelApprovedEvent
            {
                RequestId = request.Id,
                UserId = UserId,
                SkillId = request.SkillId,
                NewLevel = newLevel
            });
            return Result.Success();
        }

        /// <summary>
        /// Hạ level kỹ năng khi xác minh không đạt (mục 4.3.1): mức phạt hạ cấp gấp đôi (x2) so với
        /// mức hạ cấp thông thường, coi như một lời cảnh báo chính thức đầu tiên trên tài khoản.
        /// Lưu ý (mục 8 - vấn đề mở): số lần cảnh báo tối đa trước khi hạn chế tài khoản vẫn cần
        /// được quy định thêm ở tầng Application/Policy.
        /// </summary>
        public Result ApplyPenaltyDowngrade(Guid skillId)
        {
            var record = _records.FirstOrDefault(r => r.SkillId == skillId);
            if (record == null) return Result.Failure("Không tìm thấy kỹ năng trong hồ sơ.");

            const int normalPenaltySteps = 1;
            int penaltySteps = normalPenaltySteps * 2; // x2 theo mục 4.3.1
            int currentLevel = (int)record.Level;
            int newLevelValue = Math.Max((int)SkillLevel.Beginner, currentLevel - penaltySteps);

            record.SetLevel((SkillLevel)newLevelValue);

            AddDomainEvent(new SkillPenaltyAppliedEvent
            {
                UserId = UserId,
                SkillId = skillId,
                PreviousLevel = (SkillLevel)currentLevel,
                NewLevel = (SkillLevel)newLevelValue,
                PenaltyMultiplier = 2
            });

            return Result.Success();
        }

        /// <summary>
        /// [MỚI - fix, mục 4.3.2] Áp dụng kết quả một đề xuất phản ánh kỹ năng do CÔNG TY chủ động
        /// khởi xướng (CompanySkillReflectionRequest), SAU KHI đã xác minh qua bài kiểm tra hệ thống.
        /// Cố tình KHÔNG dùng lại ApplyLevelUp ở trên vì ApplyLevelUp luôn phát sinh
        /// SkillLevelApprovedEvent — sai ngữ nghĩa cho trường hợp Down (hạ cấp không phải một "phê
        /// duyệt nâng cấp"). Việc gửi Notification cho luồng này do SkillReflectionAppliedEvent đảm
        /// nhiệm riêng (xem SkillReflectionAppliedEventHandler), nên phương thức này không phát sinh
        /// thêm domain event nào khác.
        /// </summary>
        public Result ApplyCompanyReflectionResult(
            Guid skillId,
            SkillLevel resultLevel,
            bool isNewSkill,
            SkillLevel? expectedCurrentLevel)
        {
            if (skillId == Guid.Empty || !Enum.IsDefined(resultLevel))
                return Result.Failure("Thông tin kỹ năng áp dụng không hợp lệ.");
            var record = _records.FirstOrDefault(r => r.SkillId == skillId);

            if (isNewSkill)
            {
                if (expectedCurrentLevel.HasValue)
                    return Result.Failure("Kỹ năng mới không được có level hiện tại.");
                if (record != null)
                    return Result.Failure("Kỹ năng đã tồn tại trong hồ sơ, không thể bổ sung lại như kỹ năng mới.");

                _records.Add(UserSkillRecord.Create(skillId, resultLevel));
                return Result.Success();
            }

            if (record == null)
                return Result.Failure("Không tìm thấy kỹ năng trong hồ sơ.");
            if (!expectedCurrentLevel.HasValue || record.Level != expectedCurrentLevel.Value)
                return Result.Failure("Level hiện tại đã thay đổi kể từ khi tạo đề xuất.");

            record.SetLevel(resultLevel);
            return Result.Success();
        }
    }

    /// <summary>Một dòng kỹ năng trong hồ sơ cá nhân của User.</summary>
    public class UserSkillRecord
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid SkillId { get; private set; }
        public SkillLevel Level { get; private set; }

        private readonly List<Guid> _endorserIds = new();
        public IReadOnlyCollection<Guid> EndorserIds => _endorserIds.AsReadOnly();
        public int EndorsementCount => _endorserIds.Count;

        private UserSkillRecord() { }

        private UserSkillRecord(Guid skillId, SkillLevel level)
        {
            SkillId = skillId;
            Level = level;
        }

        public static UserSkillRecord Create(Guid skillId, SkillLevel level) => new(skillId, level);

        public Result AddEndorsement(Guid endorserId)
        {
            if (endorserId == Guid.Empty)
                return Result.Failure("EndorserId không hợp lệ.");
            if (_endorserIds.Contains(endorserId))
                return Result.Failure("Người này đã xác nhận kỹ năng trước đó.");
            _endorserIds.Add(endorserId);
            return Result.Success();
        }

        public void SetLevel(SkillLevel level)
        {
            if (!Enum.IsDefined(level))
                throw new ArgumentOutOfRangeException(nameof(level), "Skill level is invalid.");
            Level = level;
        }
    }
}
