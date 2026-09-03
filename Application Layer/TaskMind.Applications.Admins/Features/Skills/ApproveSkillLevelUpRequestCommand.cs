// ApproveSkillLevelUpRequestCommand.cs
// [CẬP NHẬT - fix]
//  1) request.Approve() không còn tự phát sinh SkillLevelApprovedEvent (xem SkillLevelUpRequest.cs) —
//     trước đây cả nó lẫn profile.ApplyLevelUp() cùng raise một event, khiến
//     SkillLevelApprovedEventHandler chạy 2 lần / gửi trùng Notification + email cho cùng một lần duyệt.
//  2) Thêm fallback Notification cho trường hợp hiếm SkillProfile == null (không tìm thấy hồ sơ kỹ
//     năng) — nếu không, sau khi bỏ event ở (1), user sẽ không nhận được bất kỳ thông báo nào.
//  3) Ghi SkillHistoryEntry (mục 4.3.3) — trước đây hoàn toàn không được ghi ở luồng này dù entity/
//     DbSet đã sẵn sàng.
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

            if (profile != null)
            {
                // Nguồn phát SkillLevelApprovedEvent DUY NHẤT — SkillLevelApprovedEventHandler sẽ lo
                // phần Notification/email cho user.
                profile.ApplyLevelUp(request.SkillId, newLevel);
            }
            else
            {
                // [MỚI - fix] Fallback hiếm gặp: không tìm thấy SkillProfile nên không có event nào
                // được phát sinh — vẫn phải báo cho user biết yêu cầu đã được duyệt.
                var fallbackNotif = Notification.Create(
                    request.UserId,
                    "Nâng cấp độ kỹ năng thành công",
                    $"Yêu cầu nâng cấp kỹ năng của bạn đã được duyệt lên mức {newLevel}.",
                    NotificationType.Success);

                if (fallbackNotif.IsSuccess)
                    _dbContext.Notifications.Add(fallbackNotif.Data!);
            }

            var auditResult = AuditLog.Record(command.ApproverAdminId, "SkillLevelUpApproved", nameof(SkillLevelUpRequest), request.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            // [MỚI - fix, mục 4.3.3] Ghi nhận lịch sử thay đổi kỹ năng.
            var responsibleAccountId = request.ApproverAccountId != Guid.Empty ? request.ApproverAccountId : command.ApproverAdminId;
            var historyResult = SkillHistoryEntry.Record(
                userId: request.UserId,
                skillId: request.SkillId,
                changeSource: SkillChangeSource.UserInitiated,
                responsibleAccountId: responsibleAccountId,
                outcome: SkillHistoryOutcome.Applied,
                levelBefore: request.CurrentLevel,
                levelAfter: newLevel,
                relatedSubmissionId: request.SubmissionId,
                relatedRequestId: request.Id);

            if (historyResult.IsSuccess)
                _dbContext.SkillHistoryEntries.Add(historyResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Duyệt yêu cầu nâng cấp độ kỹ năng thành công");
        }
    }
}