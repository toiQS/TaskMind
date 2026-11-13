using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.ApplicationLayer.Commons;
using TaskMind.DomainLayer.TaskMind.Domain.Entities;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.ApplicationLayer.Features.Projects;

public sealed record ProjectRequest(
    string Title,
    string Description,
    string SourceType,
    Guid? OwningEntityId,
    bool IsExchangeProject = false);
public sealed record UpdateProjectRequest(string Title, string Description);

public sealed record ProjectDto(
    Guid Id,
    string Title,
    string Description,
    string SourceType,
    Guid? OwningEntityId,
    bool IsExchangeProject,
    string Status,
    int ActiveMemberCount,
    DateTime CreatedAtUtc);

public sealed record ProjectPageDto(
    IReadOnlyCollection<ProjectDto> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record ProjectMemberRequest(Guid AccountId, string Role);
public sealed record ProjectMemberRoleRequest(string Role);
public sealed record ProjectReasonRequest(string? Reason);
public sealed record ProjectMemberDto(
    Guid Id,
    Guid AccountId,
    string DisplayName,
    string Role,
    bool IsActive,
    DateTime JoinedAt,
    DateTime? LeftAt);
public sealed record ProjectMemberPageDto(
    IReadOnlyCollection<ProjectMemberDto> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record ListProjectsQuery(
    string? Source,
    string? Status,
    string? Keyword,
    int Page,
    int PageSize) : IRequest<ServiceResult<ProjectPageDto>>;
public sealed record CreateProjectCommand(ProjectRequest Request) : IRequest<ServiceResult<Guid>>;
public sealed record GetProjectQuery(Guid ProjectId) : IRequest<ServiceResult<ProjectDto>>;
public sealed record UpdateProjectCommand(Guid ProjectId, string Title, string Description)
    : IRequest<ServiceResult>;
public sealed record ProjectStatusCommand(Guid ProjectId, ProjectStatus Status, bool Force, string? Reason)
    : IRequest<ServiceResult>;
public sealed record ListProjectMembersQuery(Guid ProjectId, string? Status, int Page, int PageSize)
    : IRequest<ServiceResult<ProjectMemberPageDto>>;
public sealed record AddProjectMemberCommand(Guid ProjectId, Guid AccountId, string Role)
    : IRequest<ServiceResult>;
public sealed record ChangeProjectMemberRoleCommand(Guid ProjectId, Guid MemberId, string Role)
    : IRequest<ServiceResult>;
public sealed record LeaveProjectCommand(Guid ProjectId, Guid MemberId)
    : IRequest<ServiceResult>;
public sealed record ForceRemoveProjectMemberCommand(Guid ProjectId, Guid MemberId, string? Reason)
    : IRequest<ServiceResult>;

public sealed class ListProjectsHandler : IRequestHandler<ListProjectsQuery, ServiceResult<ProjectPageDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public ListProjectsHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<ProjectPageDto>> Handle(ListProjectsQuery query, CancellationToken ct)
    {
        if (!ProjectRules.TryPaging(query.Page, query.PageSize, out var skip))
            return ServiceResult<ProjectPageDto>.Failure("Page hoặc PageSize không hợp lệ.");
        if (!ProjectRules.TryEnum(query.Source, out ProjectSourceType? source) ||
            !ProjectRules.TryEnum(query.Status, out ProjectStatus? status))
            return ServiceResult<ProjectPageDto>.Failure("Nguồn hoặc trạng thái dự án không hợp lệ.");

        var current = await ProjectRules.GetActiveAccount(_db, _session, ct);
        var projects = _db.Projects.AsNoTracking().Include(item => item.Members).AsQueryable();
        if (source.HasValue) projects = projects.Where(item => item.SourceType == source.Value);
        if (status.HasValue) projects = projects.Where(item => item.ProjectStatus == status.Value);
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.Trim().ToUpperInvariant();
            projects = projects.Where(item =>
                item.Title.ToUpper().Contains(keyword) || item.Description.ToUpper().Contains(keyword));
        }

        var all = await projects.ToListAsync(ct);
        var visible = new List<Project>();
        foreach (var project in all)
        {
            if (await ProjectRules.CanManage(_db, current, project, ct))
            {
                visible.Add(project);
                continue;
            }
            if (project.ProjectStatus != ProjectStatus.InProgress)
                continue;
            if (project.SourceType == ProjectSourceType.Company &&
                !await _db.Companies.AnyAsync(company =>
                    company.Id == project.OwningEntityId &&
                    company.IsVerified &&
                    company.Status == EntityStatus.Active, ct))
                continue;
            if (project.SourceType == ProjectSourceType.School &&
                !await _db.Schools.AnyAsync(school =>
                    school.Id == project.OwningEntityId &&
                    school.IsVerified &&
                    school.Status == EntityStatus.Active, ct))
                continue;
            visible.Add(project);
        }

        var items = visible.OrderByDescending(item => item.CreateAt).ThenBy(item => item.Id)
            .Skip(skip).Take(query.PageSize)
            .Select(ProjectRules.ToDto)
            .ToArray();
        return ServiceResult<ProjectPageDto>.Success(
            new ProjectPageDto(items, query.Page, query.PageSize, visible.Count));
    }
}

public sealed class CreateProjectHandler : IRequestHandler<CreateProjectCommand, ServiceResult<Guid>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public CreateProjectHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<Guid>> Handle(CreateProjectCommand command, CancellationToken ct)
    {
        if (!ProjectRules.TryEnum(command.Request.SourceType, out ProjectSourceType? source) || !source.HasValue)
            return ServiceResult<Guid>.Failure("Nguồn dự án không hợp lệ.");
        var owner = await ProjectRules.GetActiveAccount(_db, _session, ct);
        if (owner is null) return ServiceResult<Guid>.Forbidden();
        if (!await ProjectRules.CanCreate(_db, owner, source.Value, command.Request.OwningEntityId, ct))
            return ServiceResult<Guid>.Forbidden("Tài khoản hiện tại không được phép tạo dự án thuộc nguồn đã chọn.");
        var project = Project.Create(
            command.Request.Title,
            command.Request.Description,
            source.Value,
            owner.Id,
            command.Request.OwningEntityId,
            command.Request.IsExchangeProject);
        if (!project.IsSuccess || project.Data is null)
            return ServiceResult<Guid>.Failure(project.Message);
        _db.Projects.Add(project.Data);
        await _db.SaveChangesAsync(ct);
        return ServiceResult<Guid>.Success(project.Data.Id, "Đã tạo dự án.");
    }
}

public sealed class GetProjectHandler : IRequestHandler<GetProjectQuery, ServiceResult<ProjectDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public GetProjectHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<ProjectDto>> Handle(GetProjectQuery query, CancellationToken ct)
    {
        var project = await _db.Projects.AsNoTracking().Include(item => item.Members)
            .SingleOrDefaultAsync(item => item.Id == query.ProjectId, ct);
        if (project is null) return ServiceResult<ProjectDto>.NotFound("Không tìm thấy dự án.");
        var current = await ProjectRules.GetActiveAccount(_db, _session, ct);
        if (!await ProjectRules.CanView(_db, current, project, ct))
            return ServiceResult<ProjectDto>.NotFound("Không tìm thấy dự án.");
        return ServiceResult<ProjectDto>.Success(ProjectRules.ToDto(project));
    }
}

public sealed class UpdateProjectHandler : IRequestHandler<UpdateProjectCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public UpdateProjectHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(UpdateProjectCommand command, CancellationToken ct)
    {
        var project = await _db.Projects.SingleOrDefaultAsync(item => item.Id == command.ProjectId, ct);
        if (project is null) return ServiceResult.NotFound("Không tìm thấy dự án.");
        var account = await ProjectRules.GetActiveAccount(_db, _session, ct);
        if (!await ProjectRules.CanManage(_db, account, project, ct))
            return ServiceResult.Forbidden("Chỉ Owner, Project Manager hoặc quản trị viên sở hữu mới có thể cập nhật.");
        var result = project.UpdateDetails(command.Title, command.Description);
        if (!result.IsSuccess) return ServiceResult.Failure(result.Message);
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success("Đã cập nhật thông tin dự án.");
    }
}

public sealed class ProjectStatusHandler : IRequestHandler<ProjectStatusCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public ProjectStatusHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(ProjectStatusCommand command, CancellationToken ct)
    {
        var project = await _db.Projects.Include(item => item.Members)
            .SingleOrDefaultAsync(item => item.Id == command.ProjectId, ct);
        if (project is null) return ServiceResult.NotFound("Không tìm thấy dự án.");
        var actor = await ProjectRules.GetActiveAccount(_db, _session, ct);
        if (command.Force)
        {
            if (actor is not Admin { IsVerified: true, Status: EntityStatus.Active })
                return ServiceResult.Forbidden();
        }
        else if (!await ProjectRules.CanManage(_db, actor, project, ct))
        {
            return ServiceResult.Forbidden("Chỉ Owner, Project Manager hoặc quản trị viên sở hữu mới có thể đổi trạng thái.");
        }

        var transition = command.Status switch
        {
            ProjectStatus.Paused when command.Force => project.Pause(),
            ProjectStatus.Paused => project.Pause(),
            ProjectStatus.InProgress => project.Resume(),
            ProjectStatus.Completed => project.Complete(),
            ProjectStatus.Cancelled => project.Cancel(),
            _ => TaskMind.DomainLayer.TaskMind.Domain.Commons.Result.Result.Failure("Trạng thái dự án không hợp lệ.")
        };
        if (!transition.IsSuccess) return ServiceResult.Failure(transition.Message);

        if (command.Force)
        {
            var audit = AuditLog.Record(
                actor!.Id,
                command.Status == ProjectStatus.Paused ? "ProjectForcePaused" : "ProjectForceCancelled",
                "Project",
                project.Id);
            if (!audit.IsSuccess || audit.Data is null) return ServiceResult.Error(audit.Message);
            _db.AuditLogs.Add(audit.Data);
        }

        if (command.Status is ProjectStatus.Completed or ProjectStatus.Cancelled)
            await ProjectResourceRules.RevokeProjectWorkspacesAsync(_db, project.Id, ct);

        var notice = ProjectRules.StatusMessage(project, command.Status, command.Reason);
        foreach (var accountId in project.Members.Where(member => member.IsActive).Select(member => member.AccountId))
        {
            var notification = Notification.Create(
                accountId,
                $"Cập nhật dự án: {project.Title}",
                notice,
                command.Status is ProjectStatus.Cancelled or ProjectStatus.Paused
                    ? NotificationType.Warning
                    : NotificationType.System);
            if (!notification.IsSuccess || notification.Data is null)
                return ServiceResult.Error(notification.Message);
            _db.Notifications.Add(notification.Data);
        }

        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success($"Đã cập nhật trạng thái dự án thành {command.Status}.");
    }
}

public sealed class ListProjectMembersHandler
    : IRequestHandler<ListProjectMembersQuery, ServiceResult<ProjectMemberPageDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public ListProjectMembersHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<ProjectMemberPageDto>> Handle(ListProjectMembersQuery query, CancellationToken ct)
    {
        if (!ProjectRules.TryPaging(query.Page, query.PageSize, out var skip))
            return ServiceResult<ProjectMemberPageDto>.Failure("Page hoặc PageSize không hợp lệ.");
        if (!ProjectRules.TryMemberStatus(query.Status, out var memberStatus))
            return ServiceResult<ProjectMemberPageDto>.Failure("Status chỉ nhận Active, Left hoặc All.");
        var project = await _db.Projects.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == query.ProjectId, ct);
        if (project is null) return ServiceResult<ProjectMemberPageDto>.NotFound("Không tìm thấy dự án.");
        var current = await ProjectRules.GetActiveAccount(_db, _session, ct);
        var isMember = current is not null &&
                       await _db.ProjectMembers.AsNoTracking().AnyAsync(item =>
                           EF.Property<Guid>(item, "ProjectId") == project.Id &&
                           item.AccountId == current.Id &&
                           item.LeftAt == null, ct);
        if (!isMember && !await ProjectRules.CanManage(_db, current, project, ct))
            return ServiceResult<ProjectMemberPageDto>.Forbidden();

        var members = _db.ProjectMembers.AsNoTracking().Where(item => EF.Property<Guid>(item, "ProjectId") == project.Id);
        if (memberStatus == "Active") members = members.Where(item => item.LeftAt == null);
        else if (memberStatus == "Left") members = members.Where(item => item.LeftAt != null);
        var total = await members.CountAsync(ct);
        var rows = await (
                from member in members
                join account in _db.Accounts.AsNoTracking() on member.AccountId equals account.Id
                orderby member.JoinedAt, member.Id
                select new ProjectMemberDto(
                    member.Id,
                    member.AccountId,
                    account.Profile.NickName,
                    member.Role.ToString(),
                    member.LeftAt == null,
                    member.JoinedAt,
                    member.LeftAt))
            .Skip(skip).Take(query.PageSize).ToListAsync(ct);
        return ServiceResult<ProjectMemberPageDto>.Success(
            new ProjectMemberPageDto(rows, query.Page, query.PageSize, total));
    }
}

public sealed class AddProjectMemberHandler : IRequestHandler<AddProjectMemberCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public AddProjectMemberHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(AddProjectMemberCommand command, CancellationToken ct)
    {
        if (!ProjectRules.TryEnum(command.Role, out ProjectRole? role) || !role.HasValue ||
            role.Value == ProjectRole.Owner)
            return ServiceResult.Failure("Vai trò thành viên dự án không hợp lệ.");
        var project = await _db.Projects.Include(item => item.Members)
            .SingleOrDefaultAsync(item => item.Id == command.ProjectId, ct);
        if (project is null) return ServiceResult.NotFound("Không tìm thấy dự án.");
        var actor = await ProjectRules.GetActiveAccount(_db, _session, ct);
        if (!await ProjectRules.CanManage(_db, actor, project, ct))
            return ServiceResult.Forbidden("Chỉ Owner, Project Manager hoặc quản trị viên sở hữu mới có thể thêm thành viên.");
        var target = await _db.Accounts.SingleOrDefaultAsync(item =>
            item.Id == command.AccountId &&
            item.IsVerified &&
            item.Status == EntityStatus.Active, ct);
        if (target is null || !await ProjectRules.IsEligibleMember(_db, target, project, ct))
            return ServiceResult.Failure("Tài khoản thành viên không tồn tại, không hoạt động hoặc không thuộc phạm vi dự án.");
        var added = project.AssignMember(target.Id, role.Value);
        if (!added.IsSuccess) return ServiceResult.Failure(added.Message);
        var notification = Notification.Create(
            target.Id,
            "Bạn đã được thêm vào dự án",
            $"Bạn đã được thêm vào dự án {project.Title} với vai trò {role.Value}.",
            NotificationType.Success);
        if (!notification.IsSuccess || notification.Data is null)
            return ServiceResult.Error(notification.Message);
        _db.Notifications.Add(notification.Data);
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success("Đã thêm thành viên vào dự án.");
    }
}

public sealed class ChangeProjectMemberRoleHandler
    : IRequestHandler<ChangeProjectMemberRoleCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public ChangeProjectMemberRoleHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(ChangeProjectMemberRoleCommand command, CancellationToken ct)
    {
        if (!ProjectRules.TryEnum(command.Role, out ProjectRole? role) || !role.HasValue ||
            role.Value == ProjectRole.Owner)
            return ServiceResult.Failure("Vai trò thành viên dự án không hợp lệ.");
        var project = await _db.Projects.Include(item => item.Members)
            .SingleOrDefaultAsync(item => item.Id == command.ProjectId, ct);
        if (project is null) return ServiceResult.NotFound("Không tìm thấy dự án.");
        var member = project.Members.SingleOrDefault(item =>
            item.Id == command.MemberId && item.IsActive);
        if (member is null) return ServiceResult.NotFound("Không tìm thấy thành viên đang hoạt động.");
        var actor = await ProjectRules.GetActiveAccount(_db, _session, ct);
        if (!await ProjectRules.CanManage(_db, actor, project, ct))
            return ServiceResult.Forbidden();
        var changed = project.ChangeMemberRole(member.AccountId, role.Value);
        if (!changed.IsSuccess) return ServiceResult.Failure(changed.Message);
        var notification = Notification.Create(
            member.AccountId,
            "Vai trò dự án đã thay đổi",
            $"Vai trò của bạn trong dự án {project.Title} đã được đổi thành {role.Value}.",
            NotificationType.System);
        if (!notification.IsSuccess || notification.Data is null)
            return ServiceResult.Error(notification.Message);
        _db.Notifications.Add(notification.Data);
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success("Đã đổi vai trò thành viên dự án.");
    }
}

public sealed class LeaveProjectHandler : IRequestHandler<LeaveProjectCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public LeaveProjectHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(LeaveProjectCommand command, CancellationToken ct)
    {
        var actor = await ProjectRules.GetActiveAccount(_db, _session, ct);
        if (actor is null) return ServiceResult.Forbidden();
        var project = await _db.Projects.Include(item => item.Members)
            .SingleOrDefaultAsync(item => item.Id == command.ProjectId, ct);
        if (project is null) return ServiceResult.NotFound("Không tìm thấy dự án.");
        var membership = project.Members.SingleOrDefault(item =>
            item.Id == command.MemberId && item.IsActive);
        if (membership is null) return ServiceResult.NotFound("Không tìm thấy thành viên đang hoạt động.");
        if (membership.AccountId != actor.Id)
            return ServiceResult.Forbidden("Bạn chỉ có thể tự rời khỏi dự án bằng tài khoản hiện tại.");
        var result = project.RemoveMember(actor.Id);
        if (!result.IsSuccess) return ServiceResult.Failure(result.Message);
        await ProjectResourceRules.RevokeMemberWorkspacesAsync(_db, project.Id, actor.Id, ct);
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success("Đã rời dự án.");
    }
}

public sealed class ForceRemoveProjectMemberHandler
    : IRequestHandler<ForceRemoveProjectMemberCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public ForceRemoveProjectMemberHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(ForceRemoveProjectMemberCommand command, CancellationToken ct)
    {
        var admin = await ProjectRules.GetSystemAdmin(_db, _session, ct);
        if (admin is null) return ServiceResult.Forbidden();
        var project = await _db.Projects.Include(item => item.Members)
            .SingleOrDefaultAsync(item => item.Id == command.ProjectId, ct);
        if (project is null) return ServiceResult.NotFound("Không tìm thấy dự án.");
        var member = project.Members.SingleOrDefault(item =>
            item.Id == command.MemberId && item.IsActive);
        if (member is null) return ServiceResult.NotFound("Không tìm thấy thành viên đang hoạt động.");
        var removed = project.RemoveMember(member.AccountId);
        if (!removed.IsSuccess) return ServiceResult.Failure(removed.Message);
        await ProjectResourceRules.RevokeMemberWorkspacesAsync(_db, project.Id, member.AccountId, ct);
        var audit = AuditLog.Record(admin.Id, "ProjectMemberForceRemoved", "ProjectMember", member.Id);
        if (!audit.IsSuccess || audit.Data is null) return ServiceResult.Error(audit.Message);
        _db.AuditLogs.Add(audit.Data);
        if (member.Role != ProjectRole.Owner)
        {
            var notification = Notification.Create(
                member.AccountId,
                "Bạn đã bị gỡ khỏi dự án",
                $"Bạn đã bị gỡ khỏi dự án {project.Title}." +
                (string.IsNullOrWhiteSpace(command.Reason) ? string.Empty : $" Lý do: {command.Reason.Trim()}"),
                NotificationType.Warning);
            if (!notification.IsSuccess || notification.Data is null)
                return ServiceResult.Error(notification.Message);
            _db.Notifications.Add(notification.Data);
        }
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success("Đã gỡ thành viên khỏi dự án.");
    }
}

internal static class ProjectRules
{
    public static ProjectDto ToDto(Project project) => new(
        project.Id,
        project.Title,
        project.Description,
        project.SourceType.ToString(),
        project.OwningEntityId,
        project.IsExchangeProject,
        project.ProjectStatus.ToString(),
        project.Members.Count(member => member.IsActive),
        project.CreateAt);

    public static async Task<Account?> GetActiveAccount(
        IApplicationDbContext db, ICurrentSessionProvider session, CancellationToken ct)
    {
        var id = session.GetUserId();
        if (!id.HasValue || id.Value == Guid.Empty) return null;
        var account = await db.Accounts.SingleOrDefaultAsync(item => item.Id == id.Value, ct);
        return account is { IsVerified: true, Status: EntityStatus.Active } ? account : null;
    }

    public static async Task<Admin?> GetSystemAdmin(
        IApplicationDbContext db, ICurrentSessionProvider session, CancellationToken ct) =>
        await GetActiveAccount(db, session, ct) is Admin admin ? admin : null;

    public static bool TryEnum<TEnum>(string? value, out TEnum? result) where TEnum : struct, Enum
    {
        result = null;
        if (string.IsNullOrWhiteSpace(value)) return true;
        if (!Enum.TryParse<TEnum>(value.Trim(), true, out var parsed) ||
            !Enum.IsDefined(parsed) ||
            !string.Equals(Enum.GetName(parsed), value.Trim(), StringComparison.OrdinalIgnoreCase))
            return false;
        result = parsed;
        return true;
    }

    public static bool TryPaging(int page, int pageSize, out int skip)
    {
        skip = 0;
        if (page < 1 || pageSize is < 1 or > 100) return false;
        var offset = ((long)page - 1) * pageSize;
        if (offset > int.MaxValue) return false;
        skip = (int)offset;
        return true;
    }

    public static bool TryMemberStatus(string? value, out string? status)
    {
        status = null;
        if (string.IsNullOrWhiteSpace(value) || value.Equals("All", StringComparison.OrdinalIgnoreCase))
            return true;
        if (value.Equals("Active", StringComparison.OrdinalIgnoreCase)) status = "Active";
        else if (value.Equals("Left", StringComparison.OrdinalIgnoreCase)) status = "Left";
        else return false;
        return true;
    }

    public static async Task<bool> CanCreate(
        IApplicationDbContext db,
        Account account,
        ProjectSourceType source,
        Guid? ownerId,
        CancellationToken ct)
    {
        if (source == ProjectSourceType.OpenSource)
            return ownerId is null && account is not Admin;
        if (ownerId is null) return false;
        if (source == ProjectSourceType.Company)
            return account is AdminCompany adminCompany &&
                   adminCompany.CompanyId == ownerId &&
                   await db.Companies.AnyAsync(company =>
                       company.Id == ownerId && company.IsVerified && company.Status == EntityStatus.Active, ct);
        return account is AdminSchool adminSchool &&
               adminSchool.SchoolId == ownerId &&
               await db.Schools.AnyAsync(school =>
                   school.Id == ownerId && school.IsVerified && school.Status == EntityStatus.Active, ct);
    }

    public static async Task<bool> IsOrganizationAdmin(
        IApplicationDbContext db,
        Account? account,
        Project project,
        CancellationToken ct)
    {
        if (account is AdminCompany adminCompany &&
            project.SourceType == ProjectSourceType.Company &&
            adminCompany.CompanyId == project.OwningEntityId)
            return await db.Companies.AnyAsync(company =>
                company.Id == project.OwningEntityId &&
                company.IsVerified &&
                company.Status == EntityStatus.Active, ct);
        if (account is AdminSchool adminSchool &&
            project.SourceType == ProjectSourceType.School &&
            adminSchool.SchoolId == project.OwningEntityId)
            return await db.Schools.AnyAsync(school =>
                school.Id == project.OwningEntityId &&
                school.IsVerified &&
                school.Status == EntityStatus.Active, ct);
        return false;
    }

    public static async Task<bool> CanManage(
        IApplicationDbContext db,
        Account? account,
        Project project,
        CancellationToken ct)
    {
        if (account is null) return false;
        if (await IsOrganizationAdmin(db, account, project, ct)) return true;
        var member = await db.ProjectMembers.AsNoTracking().AnyAsync(item =>
            EF.Property<Guid>(item, "ProjectId") == project.Id &&
            item.AccountId == account.Id &&
            item.LeftAt == null &&
            (item.Role == ProjectRole.Owner || item.Role == ProjectRole.ProjectManager), ct);
        return member;
    }

    public static async Task<bool> CanView(
        IApplicationDbContext db,
        Account? account,
        Project project,
        CancellationToken ct)
    {
        if (account is not null &&
            (await CanManage(db, account, project, ct) ||
             await db.ProjectMembers.AsNoTracking().AnyAsync(item =>
                 EF.Property<Guid>(item, "ProjectId") == project.Id &&
                 item.AccountId == account.Id && item.LeftAt == null, ct)))
            return true;
        if (project.ProjectStatus != ProjectStatus.InProgress) return false;
        return project.SourceType switch
        {
            ProjectSourceType.OpenSource => true,
            ProjectSourceType.Company => await db.Companies.AnyAsync(company =>
                company.Id == project.OwningEntityId &&
                company.IsVerified &&
                company.Status == EntityStatus.Active, ct),
            ProjectSourceType.School => await db.Schools.AnyAsync(school =>
                school.Id == project.OwningEntityId &&
                school.IsVerified &&
                school.Status == EntityStatus.Active, ct),
            _ => false
        };
    }

    public static async Task<bool> IsEligibleMember(
        IApplicationDbContext db,
        Account account,
        Project project,
        CancellationToken ct) =>
        project.SourceType switch
        {
            ProjectSourceType.OpenSource => true,
            ProjectSourceType.Company => account is Staff staff &&
                staff.IsActive &&
                staff.CompanyId == project.OwningEntityId,
            ProjectSourceType.School => account is Teacher teacher &&
                teacher.IsActive &&
                teacher.SchoolId == project.OwningEntityId ||
                account is Student student &&
                student.IsActive &&
                student.SchoolId == project.OwningEntityId,
            _ => false
        };

    public static string StatusMessage(Project project, ProjectStatus status, string? reason) =>
        $"Trạng thái dự án đã thay đổi thành {status}." +
        (string.IsNullOrWhiteSpace(reason) ? string.Empty : $" Lý do: {reason.Trim()}");
}
