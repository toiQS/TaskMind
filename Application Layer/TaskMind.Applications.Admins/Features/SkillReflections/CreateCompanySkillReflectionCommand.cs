// CreateCompanySkillReflectionCommand.cs
// [CẬP NHẬT - fix]
//  1) ResolveResponsibleAccountIdAsync trước đây không kiểm tra staffAccountId có thực sự là thành
//     viên ACTIVE của ProjectId được truyền vào hay không — cho phép lấy PM của một dự án hoàn toàn
//     không liên quan tới nhân sự bị đánh giá làm người đứng tên chịu trách nhiệm.
//  2) Bổ sung chặn tạo trùng đề xuất: không cho tạo đề xuất mới nếu đã có đề xuất cùng
//     (StaffAccountId, SkillId) đang ở trạng thái PendingAdminReview/PendingVerification.
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
        public SkillLevel? ProposedLevel { get; }
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

            // [MỚI - fix] Chặn trùng: không cho tạo đề xuất mới nếu đang có đề xuất chưa xử lý xong
            // cho cùng cặp (Staff, Skill).
            var hasPending = await _dbContext.CompanySkillReflectionRequests
                .AsNoTracking()
                .AnyAsync(r => r.StaffAccountId == command.StaffAccountId
                             && r.SkillId == command.SkillId
                             && (r.Status == SkillReflectionStatus.PendingAdminReview
                                 || r.Status == SkillReflectionStatus.PendingVerification),
                    cancellationToken);
            if (hasPending)
                return ServiceResult<Guid>.Failure("Đã có một đề xuất phản ánh kỹ năng khác cho nhân sự và kỹ năng này đang chờ xử lý.");

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

                    // [MỚI - fix] Nhân sự phải thực sự là thành viên ACTIVE của dự án được dẫn chiếu —
                    // nếu không, không được dùng dự án này làm căn cứ xác định người chịu trách nhiệm,
                    // fallback thẳng xuống Admin company thay vì mượn PM của một dự án không liên quan.
                    if (evaluatedMember != null)
                    {
                        var evaluatedIsPm = evaluatedMember.Role == ProjectRole.ProjectManager;

                        if (!evaluatedIsPm)
                        {
                            var pm = project.Members.FirstOrDefault(m => m.IsActive && m.Role == ProjectRole.ProjectManager);
                            if (pm != null)
                                return pm.AccountId;
                        }
                    }
                }
            }

            // Nhân sự chính là PM, dự án không còn PM hoạt động, nhân sự không thuộc dự án được nêu,
            // hoặc không có ProjectId -> Admin company đứng tên (mục 4.3.2).
            var adminCompany = await _dbContext.AdminCompanies
                .AsNoTracking()
                .FirstOrDefaultAsync(ac => ac.CompanyId == companyId, cancellationToken);

            return adminCompany?.LinkedUserId ?? Guid.Empty;
        }
    }
}