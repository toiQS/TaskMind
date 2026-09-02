// AssignTestPaperToReflectionCommand.cs — [MỚI - fix] CompanySkillReflectionRequest.AssignTestPaper()
// tồn tại ở domain nhưng trước đây không command nào gọi tới (mục 4.3.2).
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;

namespace TaskMind.Applications.Admins.Features.SkillReflections
{
    public class AssignTestPaperToReflectionCommand : IRequest<ServiceResult>
    {
        public Guid RequestId { get; }
        public Guid TestPaperId { get; }

        public AssignTestPaperToReflectionCommand(Guid requestId, Guid testPaperId)
        {
            RequestId = requestId;
            TestPaperId = testPaperId;
        }
    }

    public class AssignTestPaperToReflectionHandler : IRequestHandler<AssignTestPaperToReflectionCommand, ServiceResult>
    {
        private readonly IApplicationDbContext _dbContext;

        public AssignTestPaperToReflectionHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult> Handle(AssignTestPaperToReflectionCommand command, CancellationToken cancellationToken)
        {
            var request = await _dbContext.CompanySkillReflectionRequests
                .FirstOrDefaultAsync(r => r.Id == command.RequestId, cancellationToken);
            if (request == null)
                return ServiceResult.NotFound("Không tìm thấy đề xuất phản ánh kỹ năng.");

            var testPaperExists = await _dbContext.TestPapers.AsNoTracking()
                .AnyAsync(t => t.Id == command.TestPaperId, cancellationToken);
            if (!testPaperExists)
                return ServiceResult.NotFound("Không tìm thấy bài kiểm tra.");

            var result = request.AssignTestPaper(command.TestPaperId);
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            await _dbContext.SaveChangesAsync(cancellationToken);
            return ServiceResult.Success("Gán bài kiểm tra xác minh thành công");
        }
    }
}
