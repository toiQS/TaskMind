using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Jobs
{
    /// <summary>
    /// Admin buộc huỷ một tin tuyển dụng vi phạm chính sách nền tảng (kiểm duyệt cấp hệ thống,
    /// khác với Company tự đóng tin qua JobPosting.Close()). Dùng chung JobPosting.Cancel().
    /// </summary>
    public class CancelJobPostingCommand : ServiceResult
    {
        public Guid JobPostingId { get; }
        public Guid ApproverAdminId { get; }
        public string? Reason { get; }

        public CancelJobPostingCommand(Guid jobPostingId, Guid approverAdminId, string? reason = null)
        {
            JobPostingId = jobPostingId;
            ApproverAdminId = approverAdminId;
            Reason = reason;
        }
    }

    public class CancelJobPostingHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public CancelJobPostingHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(CancelJobPostingCommand command, CancellationToken cancellationToken)
        {
            var posting = await _dbContext.JobPostings
                .FirstOrDefaultAsync(p => p.Id == command.JobPostingId, cancellationToken);

            if (posting == null)
                return ServiceResult.NotFound("Không tìm thấy tin tuyển dụng.");

            var result = posting.Cancel();
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            var auditResult = AuditLog.Record(command.ApproverAdminId, "JobPostingCancelledByAdmin", nameof(JobPosting), posting.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            var adminCompany = await _dbContext.AdminCompanies
                .AsNoTracking()
                .FirstOrDefaultAsync(ac => ac.CompanyId == posting.CompanyId, cancellationToken);

            if (adminCompany != null)
            {
                var notifResult = Notification.Create(
                    adminCompany.LinkedUserId,
                    "Tin tuyển dụng đã bị huỷ",
                    $"Tin tuyển dụng \"{posting.Title}\" đã bị Admin hệ thống huỷ do vi phạm chính sách nền tảng." +
                    (string.IsNullOrWhiteSpace(command.Reason) ? "" : $" Lý do: {command.Reason}"),
                    NotificationType.Warning);

                if (notifResult.IsSuccess)
                    _dbContext.Notifications.Add(notifResult.Data!);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return ServiceResult.Success("Huỷ tin tuyển dụng thành công");
        }
    }
}