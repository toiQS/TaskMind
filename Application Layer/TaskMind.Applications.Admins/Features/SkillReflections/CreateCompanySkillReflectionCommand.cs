// CreateCompanySkillReflectionCommand.cs — [MỚI - fix] Domain đã có đủ CompanySkillReflectionRequest
// .CreateUp/.CreateAdd/.CreateDown (mục 4.3.2) nhưng trước đây không command nào ở tầng Application
// gọi tới — nghĩa là công ty không có cách nào thực sự khởi tạo một đề xuất phản ánh kỹ năng.
//
// ResponsibleAccountId KHÔNG nhận trực tiếp từ client — hệ thống tự xác định theo đúng quy tắc mục
// 4.3.2 (tránh giả mạo trách nhiệm):
//   - Nếu nhân sự được đánh giá KHÔNG phải Project Manager của dự án liên quan: người đứng tên là PM
//     đang hoạt động của dự án đó.
//   - Nếu nhân sự chính LÀ PM, dự án không còn PM đang hoạt động, hoặc không có ProjectId: người đứng
//     tên là Admin company (LinkedUserId) của công ty.
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.Applications.Commons;
using TaskMind.Domain.Commons.Result;
using TaskMind.Domain.Entities;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Features.SkillReflections
{
    public class CreateCompanySkillReflectionCommand : IRequest<ServiceResult<Guid>>
    {
        public Guid CompanyId { get; }
        public Guid StaffAccountId { get; }
        public Guid SkillId { get; }
        public SkillReflectionType ReflectionType { get; }
        public Guid? ProjectId { get; }
        public string EvidenceDescription { get; }

        /// <summary>Bắt buộc với Up/Add — level đề xuất/level khởi điểm.</summary>
        public SkillLevel? ProposedLevel { get; }

        /// <summary>Chỉ có ý nghĩa với Down.</summary>
        public int? IncidentFrequency { get; }

        public CreateCompanySkillReflectionCommand(
            Guid companyId, Guid staffAccountId, Guid skillId, SkillReflectionType reflectionType,
            string evidenceDescription, Guid? projectId = null, SkillLevel? proposedLevel = null, int? incidentFrequency = null)
        {
            CompanyId = companyId;
            StaffAccountId = staffAccountId;
            SkillId = skillId;
            ReflectionType = reflectionType;
            EvidenceDescription = evidenceDescription;
            ProjectId = projectId;
            ProposedLevel = proposedLevel;
            IncidentFrequency = incidentFrequency;
        }
    }

    public class CreateCompanySkillReflectionHandler : IRequestHandler<CreateCompanySkillReflectionCommand, ServiceResult<Guid>>
    {
        private readonly IApplicationDbContext _dbContext;

        public CreateCompanySkillReflectionHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceResult<Guid>> Handle(CreateCompanySkillReflectionCommand command, CancellationToken cancellationToken)
        {
            var staff = await _dbContext.Staffs
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == command.StaffAccountId, cancellationToken);

            if (staff == null)
                return ServiceResult<Guid>.NotFound("Không tìm thấy nhân sự.");

            if (staff.CompanyId != command.CompanyId)
                return ServiceResult<Guid>.Failure("Nhân sự không thuộc công ty này.");

            var skillExists = await _dbContext.Skills.AsNoTracking()
                .AnyAsync(s => s.Id == command.SkillId, cancellationToken);
            if (!skillExists)
                return ServiceResult<Guid>.NotFound("Không tìm thấy kỹ năng.");

            var responsibleAccountId = await ResolveResponsibleAccountIdAsync(
                command.CompanyId, command.StaffAccountId, command.ProjectId, cancellationToken);

            if (responsibleAccountId == Guid.Empty)
                return ServiceResult<Guid>.Failure("Không xác định được người đứng tên chịu trách nhiệm đề xuất (thiếu Admin company liên kết).");

            Result<CompanySkillReflectionRequest> requestResult;

            switch (command.ReflectionType)
            {
                case SkillReflectionType.Up:
                    if (command.ProposedLevel is null)
                        return ServiceResult<Guid>.Failure("Phải xác định level đề xuất đối với Up.");
                    requestResult = CompanySkillReflectionRequest.CreateUp(
                        command.CompanyId, staff.LinkedUserId, staff.Id, command.SkillId, responsibleAccountId,
                        command.ProjectId, command.EvidenceDescription, command.ProposedLevel.Value);
                    break;

                case SkillReflectionType.Add:
                    if (command.ProposedLevel is null)
                        return ServiceResult<Guid>.Failure("Phải xác định level khởi điểm đối với Add.");
                    requestResult = CompanySkillReflectionRequest.CreateAdd(
                        command.CompanyId, staff.LinkedUserId, staff.Id, command.SkillId, responsibleAccountId,
                        command.ProjectId, command.EvidenceDescription, command.ProposedLevel.Value);
                    break;

                case SkillReflectionType.Down:
                    var profile = await _dbContext.SkillProfiles.AsNoTracking()
                        .FirstOrDefaultAsync(p => p.UserId == staff.LinkedUserId, cancellationToken);
                    var currentRecord = profile?.Records.FirstOrDefault(r => r.SkillId == command.SkillId);
                    if (currentRecord == null)
                        return ServiceResult<Guid>.Failure("Nhân sự chưa có kỹ năng này trong hồ sơ, không thể đề xuất hạ cấp.");

                    requestResult = CompanySkillReflectionRequest.CreateDown(
                        command.CompanyId, staff.LinkedUserId, staff.Id, command.SkillId, responsibleAccountId,
                        command.ProjectId, command.EvidenceDescription, currentRecord.Level, command.IncidentFrequency);
                    break;

                default:
                    return ServiceResult<Guid>.Failure("Loại phản ánh kỹ năng không hợp lệ.");
            }

            if (!requestResult.IsSuccess)
                return ServiceResult<Guid>.Failure(requestResult.Message);

            _dbContext.CompanySkillReflectionRequests.Add(requestResult.Data!);

            var auditResult = AuditLog.Record(
                responsibleAccountId,
                $"CompanySkillReflection{command.ReflectionType}Requested",
                nameof(CompanySkillReflectionRequest),
                requestResult.Data!.Id);
            if (auditResult.IsSuccess)
                _dbContext.AuditLogs.Add(auditResult.Data!);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult<Guid>.Success(requestResult.Data!.Id, "Tạo đề xuất phản ánh kỹ năng thành công");
        }

        private async Task<Guid> ResolveResponsibleAccountIdAsync(
            Guid companyId, Guid staffAccountId, Guid? projectId, CancellationToken cancellationToken)
        {
            if (projectId.HasValue)
            {
                var project = await _dbContext.Projects
                    .AsNoTracking()
                    .Include(p => p.Members)
                    .FirstOrDefaultAsync(p => p.Id == projectId.Value, cancellationToken);

                if (project != null)
                {
                    var evaluatedMember = project.Members.FirstOrDefault(m => m.AccountId == staffAccountId && m.IsActive);
                    var evaluatedIsPm = evaluatedMember?.Role == ProjectRole.ProjectManager;

                    if (!evaluatedIsPm)
                    {
                        var pm = project.Members.FirstOrDefault(m => m.IsActive && m.Role == ProjectRole.ProjectManager);
                        if (pm != null)
                            return pm.AccountId;
                    }
                }
            }

            // Nhân sự chính là PM, dự án không còn PM hoạt động, hoặc không có ProjectId -> Admin company đứng tên (mục 4.3.2).
            var adminCompany = await _dbContext.AdminCompanies
                .AsNoTracking()
                .FirstOrDefaultAsync(ac => ac.CompanyId == companyId, cancellationToken);

            return adminCompany?.LinkedUserId ?? Guid.Empty;
        }
    }
}