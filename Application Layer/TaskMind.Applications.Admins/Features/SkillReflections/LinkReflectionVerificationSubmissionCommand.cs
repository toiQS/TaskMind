// LinkReflectionVerificationSubmissionCommand.cs — [MỚI - fix] CompanySkillReflectionRequest
// .LinkVerificationSubmission() tồn tại ở domain nhưng trước đây không command nào gọi tới.
// Sau khi liên kết, SubmissionGradedEventHandler (đã cập nhật) sẽ TỰ ĐỘNG gọi ApplyVerificationResult
// khi Submission này được chấm điểm — không cần thêm thao tác thủ công nào khác.
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.SkillReflections
{
    public class LinkReflectionVerificationSubmissionCommand : IRequest<ServiceResult>
    {
        public Guid RequestId { get; }
        public Guid SubmissionId { get; }

        public LinkReflectionVerificationSubmissionCommand(Guid requestId, Guid submissionId)
        {
            RequestId = requestId;
            SubmissionId = submissionId;
        }
    }

    public class LinkReflectionVerificationSubmissionHandler : IRequestHandler<LinkReflectionVerificationSubmissionCommand, ServiceResult>
    {
        private readonly IApplicationDbContext _dbContext;

        public LinkReflectionVerificationSubmissionHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(LinkReflectionVerificationSubmissionCommand command, CancellationToken cancellationToken)
        {
            var request = await _dbContext.CompanySkillReflectionRequests
                .FirstOrDefaultAsync(r => r.Id == command.RequestId, cancellationToken);
            if (request == null)
                return ServiceResult.NotFound("Không tìm thấy đề xuất phản ánh kỹ năng.");

            var submissionExists = await _dbContext.Submissions.AsNoTracking()
                .AnyAsync(s => s.Id == command.SubmissionId, cancellationToken);
            if (!submissionExists)
                return ServiceResult.NotFound("Không tìm thấy bài làm.");

            var result = request.LinkVerificationSubmission(command.SubmissionId);
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            await _dbContext.SaveChangesAsync(cancellationToken);
            return ServiceResult.Success("Liên kết bài làm xác minh thành công");
        }
    }
}
