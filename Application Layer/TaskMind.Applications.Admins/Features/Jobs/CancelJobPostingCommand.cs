using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;

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

            // TODO: AuditLog.Record(command.ApproverAdminId, "JobPostingCancelledByAdmin", nameof(JobPosting), posting.Id)
            // TODO: gửi Notification cho Company khi bổ sung tài liệu chi tiết luồng kiểm duyệt.

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult.Success("Huỷ tin tuyển dụng thành công");
        }
    }
}