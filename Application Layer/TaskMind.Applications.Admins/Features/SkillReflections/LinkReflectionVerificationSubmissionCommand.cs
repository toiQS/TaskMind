// LinkReflectionVerificationSubmissionCommand.cs
// [CẬP NHẬT - fix bảo mật] Trước đây chỉ check Submission tồn tại (AnyAsync), không xác minh
// Submission đó có thực sự thuộc đúng UserId của đề xuất và đúng TestPaper đã gán hay không —
// cho phép link nhầm/link cố ý bài làm của người khác để "hợp thức hoá" một đề xuất hạ/nâng cấp
// kỹ năng, phá vỡ nguyên tắc xác minh khách quan (mục 4.3.2).
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

            var submission = await _dbContext.Submissions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == command.SubmissionId, cancellationToken);
            if (submission == null)
                return ServiceResult.NotFound("Không tìm thấy bài làm.");

            // [MỚI - fix] Bài làm phải của đúng người bị đánh giá trong đề xuất.
            if (submission.UserId != request.UserId)
                return ServiceResult.Failure("Bài làm không thuộc về nhân sự trong đề xuất phản ánh kỹ năng này.");

            // [MỚI - fix] Nếu đề xuất đã được gán TestPaper cụ thể, bài làm phải làm đúng bài đó —
            // tránh trường hợp lấy một bài làm hợp lệ nhưng không đúng phạm vi/level đã xác định.
            if (request.AssignedTestPaperId.HasValue && submission.TestPaperId != request.AssignedTestPaperId.Value)
                return ServiceResult.Failure("Bài làm không khớp với bài kiểm tra đã gán cho đề xuất này.");

            var result = request.LinkVerificationSubmission(command.SubmissionId);
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            await _dbContext.SaveChangesAsync(cancellationToken);
            return ServiceResult.Success("Liên kết bài làm xác minh thành công");
        }
    }
}