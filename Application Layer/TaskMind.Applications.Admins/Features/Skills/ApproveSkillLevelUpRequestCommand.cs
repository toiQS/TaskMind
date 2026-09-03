// ApproveSkillLevelUpRequestCommand.cs
// [CẬP NHẬT - fix]
//  ... (giữ các fix cũ) ...
//  4) [MỚI] Nhánh fallback khi profile == null trước đây vẫn ghi SkillHistoryEntry với
//     Outcome = Applied / LevelAfter = newLevel dù thực chất KHÔNG có gì được áp dụng lên hồ sơ nào
//     cả (vì không có profile để sửa) — sai lệch dữ liệu lịch sử theo đúng loại lỗi mà
//     SkillReflectionAppliedEventHandler đã né. Giờ tự động khởi tạo SkillProfile mới nếu thiếu,
//     rồi mới ApplyLevelUp thật sự lên nó — nhất quán với cách SkillReflectionAppliedEventHandler xử lý.
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Skills
{
    public class ApproveSkillLevelUpRequestCommand : IRequest<ServiceResult>
    {
        public Guid RequestId { get; }
        public Guid ApproverAdminId { get; }

        public ApproveSkillLevelUpRequestCommand(Guid requestId, Guid approverAdminId)
        {
            RequestId = requestId;
            ApproverAdminId = approverAdminId;
        }
    }

    public class ApproveSkillLevelUpRequestHandler : IRequestHandler<ApproveSkillLevelUpRequestCommand, ServiceResult>
    {
        private readonly IApplicationDbContext _dbContext;

        public ApproveSkillLevelUpRequestHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(ApproveSkillLevelUpRequestCommand command, CancellationToken cancellationToken)
        {
            var request = await _dbContext.SkillLevelUpRequests
                .FirstOrDefaultAsync(r => r.Id == command.RequestId, cancellationToken);

            if (request == null)
                return ServiceResult.NotFound("Không tìm thấy yêu cầu nâng cấp độ kỹ năng.");

            var approveResult = request.Approve();
            if (!approveResult.IsSuccess)
                return ServiceResult.Failure(approveResult.Message);

            var newLevel = (SkillLevel)Math.Min((int)SkillLevel.Expert, (int)request.CurrentLevel + 1);

            var profile = await _dbContext.SkillProfiles
                .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

            // [MỚI - fix] Thay vì fallback gửi Notification "khống" khi thiếu profile, tự khởi tạo
            // SkillProfile mới rồi áp dụng thật — đảm bảo Notification/SkillLevelApprovedEvent phản
            // ánh đúng một thay đổi đã thực sự xảy ra trên hồ sơ.
            if (profile == null)
            {
                var profileResult = SkillProfile.Create(request.UserId);
                if (profileResult.IsSuccess)
                {
                    profile = profileResult.Data!;
                    _dbContext.SkillProfiles.Add(profile);

                    // Kỹ năng chưa từng được khai báo trong hồ sơ mới tạo -> không thể ApplyLevelUp
                    // trực tiếp (ApplyLevelUp yêu cầu record đã tồn tại). Khai báo trước ở CurrentLevel
                    // của request rồi mới nâng lên newLevel, giữ đúng ngữ nghĩa "đã nâng cấp".
                    profile.DeclareSkill(request.SkillId, request.CurrentLevel);
                }
            }

            bool appliedSuccessfully = false;
            if (profile != null)
            {
                var applyResult = profile.ApplyLevelUp(request.SkillId, newLevel);
                appliedSuccessfully = applyResult.IsSuccess;
            }

            if (!appliedSuccessfully)
            {
                // Trường hợp cực hiếm (không tạo được profile) — vẫn báo cho user biết yêu cầu được
                // duyệt nhưng có sự cố kỹ thuật, không ghi lịch sử là đã áp dụng.
                var fallbackNotif = Notification.Create(
                    request.UserId,
                    "Yêu cầu nâng cấp độ kỹ năng đã được duyệt",
                    "Yêu cầu của bạn đã được duyệt nhưng hệ thống gặp sự cố khi cập nhật hồ sơ kỹ năng. Vui lòng liên hệ hỗ trợ.",
                    NotificationType.Warning);

                if (fallbackNotif.IsSuccess)
                    _dbContext.Notifications.Add(fallbackNotif.Data!);
            }

            var auditResult = AuditLog.Record(command.ApproverAdminId, "SkillLevelUpApproved", nameof(SkillLevelUpRequest), request.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            // [MỚI - fix] Chỉ ghi Outcome = Applied khi THỰC SỰ áp dụng thành công lên profile.
            var responsibleAccountId = request.ApproverAccountId != Guid.Empty ? request.ApproverAccountId : command.ApproverAdminId;
            var historyResult = SkillHistoryEntry.Record(
                userId: request.UserId,
                skillId: request.SkillId,
                changeSource: SkillChangeSource.UserInitiated,
                responsibleAccountId: responsibleAccountId,
                outcome: appliedSuccessfully ? SkillHistoryOutcome.Applied : SkillHistoryOutcome.Rejected,
                levelBefore: request.CurrentLevel,
                levelAfter: appliedSuccessfully ? newLevel : null,
                relatedSubmissionId: request.SubmissionId,
                relatedRequestId: request.Id,
                rejectionReason: appliedSuccessfully ? null : "Lỗi kỹ thuật: không thể khởi tạo/cập nhật hồ sơ kỹ năng.");

            if (historyResult.IsSuccess)
                _dbContext.SkillHistoryEntries.Add(historyResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Duyệt yêu cầu nâng cấp độ kỹ năng thành công");
        }
    }
}