// AssignTestPaperToReflectionCommand.cs — [MỚI - fix] CompanySkillReflectionRequest.AssignTestPaper()
// tồn tại ở domain nhưng trước đây không command nào gọi tới (mục 4.3.2).
//
// [CẬP NHẬT - fix]
//  1) Bổ sung kiểm tra TestPaper.OwnerType = Company và TestPaper.OwnerId = request.CompanyId — trước
//     đây chỉ check TestPaperId tồn tại, nghĩa là có thể vô tình (hoặc cố ý) gán một bài kiểm tra
//     thuộc công ty/cơ sở đào tạo KHÁC làm căn cứ xác minh, ảnh hưởng trực tiếp tới tính hợp lệ của
//     một thay đổi trên hồ sơ kỹ năng toàn cục (SkillProfile).
//  2) Bổ sung ApproverAdminId + AuditLog — gán bài kiểm tra là bước quyết định nội dung xác minh
//     trong quy trình hạ/nâng cấp kỹ năng (mục 4.3.2), cùng mức độ quan trọng như AdminAccept/
//     AdminDismiss (vốn đã có AuditLog) nhưng bước này lại không được truy vết.
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.SkillReflections
{
    public class AssignTestPaperToReflectionCommand : IRequest<ServiceResult>
    {
        public Guid RequestId { get; }
        public Guid TestPaperId { get; }
        public Guid ApproverAdminId { get; }

        public AssignTestPaperToReflectionCommand(Guid requestId, Guid testPaperId, Guid approverAdminId)
        {
            RequestId = requestId;
            TestPaperId = testPaperId;
            ApproverAdminId = approverAdminId;
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

            var testPaper = await _dbContext.TestPapers
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == command.TestPaperId, cancellationToken);
            if (testPaper == null)
                return ServiceResult.NotFound("Không tìm thấy bài kiểm tra.");

            // [MỚI - fix] Đảm bảo bài kiểm tra thuộc đúng công ty của đề xuất — tránh gán nhầm bài
            // kiểm tra không liên quan làm căn cứ xác minh thay đổi hồ sơ kỹ năng của người khác.
            if (testPaper.OwnerType != TestOwnerType.Company || testPaper.OwnerId != request.CompanyId)
                return ServiceResult.Failure("Bài kiểm tra không thuộc công ty của đề xuất phản ánh kỹ năng này.");

            var result = request.AssignTestPaper(command.TestPaperId);
            if (!result.IsSuccess)
                return ServiceResult.Failure(result.Message);

            var auditResult = AuditLog.Record(
                command.ApproverAdminId,
                "SkillReflectionTestPaperAssigned",
                nameof(CompanySkillReflectionRequest),
                request.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);
            return ServiceResult.Success("Gán bài kiểm tra xác minh thành công");
        }
    }
}