// Application Layer/TaskMind.Applications.Admins/Features/Skills/LinkSkillLevelUpRequestSubmissionCommand.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.Skills
{
    public class LinkSkillLevelUpRequestSubmissionCommand : IRequest<ServiceResult>
    {
        public Guid RequestId { get; }
        public Guid SubmissionId { get; }

        public LinkSkillLevelUpRequestSubmissionCommand(Guid requestId, Guid submissionId)
        {
            RequestId = requestId;
            SubmissionId = submissionId;
        }
    }

    public class LinkSkillLevelUpRequestSubmissionHandler : IRequestHandler<LinkSkillLevelUpRequestSubmissionCommand, ServiceResult>
    {
        private readonly IApplicationDbContext _dbContext;

        public LinkSkillLevelUpRequestSubmissionHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(LinkSkillLevelUpRequestSubmissionCommand command, CancellationToken cancellationToken)
        {
            var request = await _dbContext.SkillLevelUpRequests
                .FirstOrDefaultAsync(r => r.Id == command.RequestId, cancellationToken);
            if (request == null)
                return ServiceResult.NotFound("Không tìm thấy yêu cầu nâng cấp độ kỹ năng.");

            if (request.RequestType != SkillLevelUpMethod.Assessment)
                return ServiceResult.Failure("Chỉ yêu cầu theo hình thức Assessment mới cần liên kết bài làm.");

            if (request.RequestStatus != SkillLevelUpRequestStatus.PendingAssessment)
                return ServiceResult.Failure("Yêu cầu không ở trạng thái chờ đánh giá, không thể liên kết bài làm.");

            var submission = await _dbContext.Submissions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == command.SubmissionId, cancellationToken);
            if (submission == null)
                return ServiceResult.NotFound("Không tìm thấy bài làm.");

            // Bài làm phải của đúng User đang yêu cầu nâng cấp — tránh mượn kết quả của người khác.
            if (submission.UserId != request.UserId)
                return ServiceResult.Failure("Bài làm không thuộc về người gửi yêu cầu này.");

            // Một Submission chỉ nên gắn với duy nhất một request — tránh SubmissionGradedEventHandler
            // xử lý nhầm/nhiều lần cho nhiều request cùng trỏ tới một SubmissionId.
            var alreadyLinkedElsewhere = await _dbContext.SkillLevelUpRequests
                .AsNoTracking()
                .AnyAsync(r => r.Id != request.Id && r.SubmissionId == command.SubmissionId, cancellationToken);
            if (alreadyLinkedElsewhere)
                return ServiceResult.Failure("Bài làm này đã được liên kết với một yêu cầu nâng cấp kỹ năng khác.");

            var result = request.LinkSubmission(command.SubmissionId);
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            await _dbContext.SaveChangesAsync(cancellationToken);
            return ServiceResult.Success("Liên kết bài làm thành công");
        }
    }
}