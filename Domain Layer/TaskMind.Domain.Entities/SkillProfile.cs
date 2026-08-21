using Microsoft.EntityFrameworkCore;
using TaskMind.Domain.Commons.Cores;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Enums;
using TaskMind.Domain.Events;

namespace TaskMind.Domain.Entities
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
            if (_records.Any(r => r.SkillId == skillId))
                return Result.Failure("Kỹ năng đã tồn tại trong hồ sơ, dùng RequestLevelUp để nâng level.");

            _records.Add(UserSkillRecord.Create(skillId, level));
            return Result.Success();
        }

        public Result Endorse(Guid skillId, Guid endorserId)
        {
            var record = _records.FirstOrDefault(r => r.SkillId == skillId);
            if (record == null) return Result.Failure("Không tìm thấy kỹ năng trong hồ sơ.");
            return record.AddEndorsement(endorserId);
        }

        /// <summary>Khởi tạo yêu cầu nâng level kỹ năng (mục 4.3.1): cần endorsement từ người có thẩm
        /// quyền cao hơn, hoặc trải qua chu trình đánh giá năng lực (Assessment context).</summary>
        public Result<SkillLevelUpRequest> RequestLevelUp(Guid skillId, Guid approverAccountId)
        {
            var record = _records.FirstOrDefault(r => r.SkillId == skillId);
            if (record == null) return Result<SkillLevelUpRequest>.Failure("Không tìm thấy kỹ năng trong hồ sơ.");
            if (record.Level == SkillLevel.Expert) return Result<SkillLevelUpRequest>.Failure("Kỹ năng đã đạt cấp cao nhất.");

            var requestResult = SkillLevelUpRequest.Create(UserId, skillId, record.Level, approverAccountId);
            if (!requestResult.IsSuccess) return requestResult;

            AddDomainEvent(new SkillLevelUpRequestedEvent
            {
                UserId = UserId,
                SkillId = skillId,
                CurrentLevel = record.Level,
                RequestId = requestResult.Data!.Id
            });

            return requestResult;
        }

        /// <summary>Áp dụng nâng level sau khi request được duyệt.</summary>
        public Result ApplyLevelUp(Guid skillId, SkillLevel newLevel)
        {
            var record = _records.FirstOrDefault(r => r.SkillId == skillId);
            if (record == null) return Result.Failure("Không tìm thấy kỹ năng trong hồ sơ.");
            record.SetLevel(newLevel);

            AddDomainEvent(new SkillLevelApprovedEvent { UserId = UserId, SkillId = skillId, NewLevel = newLevel });
            return Result.Success();
        }

        /// <summary>
        /// Hạ level kỹ năng khi xác minh không đạt (mục 4.3.1): mức phạt hạ cấp gấp đôi (x2) so với
        /// mức hạ cấp thông thường, coi như một lời cảnh báo chính thức đầu tiên trên tài khoản.
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
            if (_endorserIds.Contains(endorserId))
                return Result.Failure("Người này đã xác nhận kỹ năng trước đó.");
            _endorserIds.Add(endorserId);
            return Result.Success();
        }

        public void SetLevel(SkillLevel level) => Level = level;
    }
}
