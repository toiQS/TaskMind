using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.ApplicationLayer.Commons;
using TaskMind.ApplicationLayer.Features.Projects;
using TaskMind.DomainLayer.TaskMind.Domain.Entities;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.ApplicationLayer.Features.Organizations;

public sealed record OrganizationMemberDto(
    Guid AccountId, Guid LinkedUserId, string FirstName, string LastName,
    string NickName, bool IsActive, DateTimeOffset CreatedAtUtc, DateTimeOffset? LeftAtUtc);

public sealed record OrganizationMembersPageDto(
    IReadOnlyCollection<OrganizationMemberDto> Items, int Page, int PageSize, int TotalCount);

public sealed record ListCompanyStaffQuery(Guid CompanyId, string? Status, int Page, int PageSize)
    : IRequest<ServiceResult<OrganizationMembersPageDto>>;
public sealed record DeactivateOrganizationMemberCommand(
    Guid OrganizationId, Guid MemberId, string MemberType)
    : IRequest<ServiceResult>;
public sealed record TransferStudentCommand(Guid SourceSchoolId, Guid StudentId, Guid TargetSchoolId)
    : IRequest<ServiceResult>;
public sealed record TransferStudentRequest(Guid TargetSchoolId);

public sealed class ListCompanyStaffHandler :
    IRequestHandler<ListCompanyStaffQuery, ServiceResult<OrganizationMembersPageDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public ListCompanyStaffHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<OrganizationMembersPageDto>> Handle(
        ListCompanyStaffQuery query, CancellationToken ct)
    {
        if (!OrganizationRules.TryPaging(query.Page, query.PageSize, out var skip))
            return ServiceResult<OrganizationMembersPageDto>.Failure("Page phải từ 1 trở lên và PageSize phải từ 1 đến 100.");
        var status = query.Status?.Trim();
        if (!string.IsNullOrEmpty(status) &&
            !status.Equals("Active", StringComparison.OrdinalIgnoreCase) &&
            !status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
            return ServiceResult<OrganizationMembersPageDto>.Failure("Trạng thái nhân sự không hợp lệ.");
        var admin = await OrganizationRules.GetCurrentAccount(_db, _session, ct) as AdminCompany;
        if (admin is not { IsVerified: true, Status: EntityStatus.Active } ||
            admin.CompanyId != query.CompanyId ||
            !await _db.Companies.AnyAsync(item => item.Id == query.CompanyId &&
                item.IsVerified && item.Status == EntityStatus.Active, ct))
            return ServiceResult<OrganizationMembersPageDto>.Forbidden("Chỉ Admin của công ty mới có quyền xem danh sách nhân sự.");

        var staff = _db.Staffs.AsNoTracking().Where(item => item.CompanyId == query.CompanyId);
        if (string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase))
            staff = staff.Where(item => item.IsActive);
        else if (string.Equals(status, "Inactive", StringComparison.OrdinalIgnoreCase))
            staff = staff.Where(item => !item.IsActive);
        var total = await staff.CountAsync(ct);
        var rows = await staff.Include(item => item.Profile)
            .OrderByDescending(item => item.IsActive)
            .ThenBy(item => item.Profile.LastName)
            .ThenBy(item => item.Profile.FirstName)
            .Skip(skip).Take(query.PageSize)
            .Select(item => new OrganizationMemberDto(
                item.Id, item.LinkedUserId, item.Profile.FirstName, item.Profile.LastName,
                item.Profile.NickName, item.IsActive, item.CreatedAtUtc, item.LeftAtUtc))
            .ToArrayAsync(ct);
        return ServiceResult<OrganizationMembersPageDto>.Success(
            new OrganizationMembersPageDto(rows, query.Page, query.PageSize, total));
    }
}

public sealed class DeactivateOrganizationMemberHandler :
    IRequestHandler<DeactivateOrganizationMemberCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public DeactivateOrganizationMemberHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(DeactivateOrganizationMemberCommand command, CancellationToken ct)
    {
        var actor = await OrganizationRules.GetCurrentAccount(_db, _session, ct);
        if (actor is not { IsVerified: true, Status: EntityStatus.Active })
            return ServiceResult.Forbidden("Không xác định được tài khoản đang hoạt động.");

        if (command.MemberType == "Staff")
        {
            if (actor is not AdminCompany companyAdmin || companyAdmin.CompanyId != command.OrganizationId)
                return ServiceResult.Forbidden("Chỉ Admin công ty mới có quyền ngừng hoạt động nhân sự.");
            if (!await _db.Companies.AnyAsync(item => item.Id == command.OrganizationId &&
                    item.IsVerified && item.Status == EntityStatus.Active, ct))
                return ServiceResult.Forbidden("Công ty không ở trạng thái hoạt động.");
            var staff = await _db.Staffs.SingleOrDefaultAsync(item =>
                item.Id == command.MemberId && item.CompanyId == command.OrganizationId, ct);
            if (staff is null) return ServiceResult.NotFound("Không tìm thấy nhân sự của công ty.");
            var deactivated = staff.Deactivate();
            if (!deactivated.IsSuccess) return ServiceResult.Failure(deactivated.Message);
            var record = OrganizationRules.AddOrganizationRecord(
                _db, actor.Id, staff.Id, staff.Id, "Staff", "StaffDeactivated",
                "Tài khoản nhân sự đã ngừng hoạt động.",
                "Quản trị viên công ty đã ngừng hoạt động tài khoản của bạn.",
                NotificationType.Warning);
            if (!record.IsSuccess) return record;
        }
        else if (command.MemberType is "Teacher" or "Student")
        {
            if (actor is not AdminSchool schoolAdmin || schoolAdmin.SchoolId != command.OrganizationId)
                return ServiceResult.Forbidden("Chỉ Admin cơ sở đào tạo mới có quyền quản lý giảng viên/học viên.");
            if (!await _db.Schools.AnyAsync(item => item.Id == command.OrganizationId &&
                    item.IsVerified && item.Status == EntityStatus.Active, ct))
                return ServiceResult.Forbidden("Cơ sở đào tạo không ở trạng thái hoạt động.");
            if (command.MemberType == "Teacher")
            {
                var teacher = await _db.Teachers.SingleOrDefaultAsync(item =>
                    item.Id == command.MemberId && item.SchoolId == command.OrganizationId, ct);
                if (teacher is null) return ServiceResult.NotFound("Không tìm thấy giảng viên của cơ sở đào tạo.");
                var deactivated = teacher.Deactivate();
                if (!deactivated.IsSuccess) return ServiceResult.Failure(deactivated.Message);
                var record = OrganizationRules.AddOrganizationRecord(
                    _db, actor.Id, teacher.Id, teacher.Id, "Teacher", "TeacherDeactivated",
                    "Tài khoản giảng viên đã ngừng hoạt động.",
                    "Quản trị viên cơ sở đào tạo đã ngừng hoạt động tài khoản của bạn.",
                    NotificationType.Warning);
                if (!record.IsSuccess) return record;
            }
            else
            {
                var student = await _db.Students.SingleOrDefaultAsync(item =>
                    item.Id == command.MemberId && item.SchoolId == command.OrganizationId, ct);
                if (student is null) return ServiceResult.NotFound("Không tìm thấy học viên của cơ sở đào tạo.");
                var deactivated = student.Deactivate();
                if (!deactivated.IsSuccess) return ServiceResult.Failure(deactivated.Message);
                var record = OrganizationRules.AddOrganizationRecord(
                    _db, actor.Id, student.Id, student.Id, "Student", "StudentDeactivated",
                    "Tài khoản học viên đã ngừng hoạt động.",
                    "Quản trị viên cơ sở đào tạo đã ngừng hoạt động tài khoản của bạn.",
                    NotificationType.Warning);
                if (!record.IsSuccess) return record;
            }
        }
        else
        {
            return ServiceResult.Failure("Loại thành viên không hợp lệ.");
        }

        await ProjectResourceRules.RevokeAccountWorkspacesAsync(_db, command.MemberId, ct);
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success("Đã ngừng hoạt động thành viên.");
    }
}

public sealed class TransferStudentHandler :
    IRequestHandler<TransferStudentCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;

    public TransferStudentHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(TransferStudentCommand command, CancellationToken ct)
    {
        var actor = await OrganizationRules.GetCurrentAccount(_db, _session, ct);
        if (actor is not AdminSchool admin || admin.SchoolId != command.SourceSchoolId ||
            !admin.IsVerified || admin.Status != EntityStatus.Active)
            return ServiceResult.Forbidden("Chỉ Admin của cơ sở đào tạo hiện tại mới có quyền chuyển học viên.");
        if (!await _db.Schools.AnyAsync(item => item.Id == command.SourceSchoolId &&
                item.IsVerified && item.Status == EntityStatus.Active, ct))
            return ServiceResult.Forbidden("Cơ sở đào tạo hiện tại không ở trạng thái hoạt động.");
        if (!await _db.Schools.AnyAsync(item => item.Id == command.TargetSchoolId &&
                item.IsVerified && item.Status == EntityStatus.Active, ct))
            return ServiceResult.NotFound("Không tìm thấy cơ sở đào tạo đích đang hoạt động.");
        var student = await _db.Students.SingleOrDefaultAsync(item =>
            item.Id == command.StudentId && item.SchoolId == command.SourceSchoolId, ct);
        if (student is null) return ServiceResult.NotFound("Không tìm thấy học viên tại cơ sở đào tạo hiện tại.");
        var transferred = student.TransferTo(command.TargetSchoolId);
        if (!transferred.IsSuccess) return ServiceResult.Failure(transferred.Message);
        var record = OrganizationRules.AddOrganizationRecord(
            _db, actor.Id, student.Id, student.Id, "Student", "StudentTransferred",
            "Học viên đã được chuyển cơ sở đào tạo.",
            "Hồ sơ học viên của bạn đã được chuyển sang cơ sở đào tạo mới.",
            NotificationType.System);
        if (!record.IsSuccess) return record;
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success("Đã chuyển học viên sang cơ sở đào tạo mới.");
    }
}
