using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskMind.ApplicationLayer.Commons;
using TaskMind.DomainLayer.TaskMind.Domain.Entities;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.ApplicationLayer.Features.Projects;

public sealed record CreateProjectRepositoryRequest(string StorageProvider, string StorageRootRef);
public sealed record RegisterProjectWorkspaceRequest(string DeviceName, string Environment);
public sealed record PushProjectRevisionRequest(
    Guid WorkspaceId,
    Guid? BaseRevisionId,
    string Message,
    IReadOnlyCollection<RevisionFileEntryInput> Files);
public sealed record ReviewProjectRevisionRequest(string Result, string? Comment);
public sealed record PromoteProjectRevisionRequest(Guid RevisionId, string Environment);
public sealed record RedactProjectRevisionRequest(string Reason);

public sealed record ProjectEnvironmentDto(string Environment, Guid? CurrentRevisionId);
public sealed record ProjectRepositoryDto(
    Guid Id,
    Guid ProjectId,
    string StorageProvider,
    string StorageRootRef,
    long TotalSize,
    IReadOnlyCollection<ProjectEnvironmentDto> Environments);
public sealed record ProjectRevisionDto(
    Guid Id,
    Guid? ParentRevisionId,
    string ManifestHash,
    string Message,
    Guid AuthorAccountId,
    bool IsRedacted,
    string? RedactionReason,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyCollection<RevisionFileEntryInput> Files,
    IReadOnlyCollection<RevisionReviewDto> Reviews);
public sealed record ProjectRevisionPageDto(
    IReadOnlyCollection<ProjectRevisionDto> Items, int Page, int PageSize, int TotalCount);
public sealed record RevisionReviewDto(
    Guid ReviewerAccountId,
    string ReviewerRole,
    string Result,
    string Comment,
    DateTimeOffset ReviewedAtUtc);
public sealed record ProjectWorkspaceDto(
    Guid Id,
    string DeviceName,
    string Environment,
    Guid? BaseRevisionId,
    string Status,
    DateTimeOffset LastSyncedAtUtc);
public sealed record ProjectWorkspaceSnapshotDto(
    ProjectWorkspaceDto Workspace,
    Guid? CurrentRevisionId,
    string? ManifestHash,
    bool IsRedacted,
    IReadOnlyCollection<RevisionFileEntryInput> Files);
public sealed record RevisionPromotionDto(
    Guid RevisionId,
    string Environment,
    Guid ResponsibleAccountId,
    DateTimeOffset PromotedAtUtc);
public sealed record RevisionPromotionPageDto(
    IReadOnlyCollection<RevisionPromotionDto> Items, int Page, int PageSize, int TotalCount);

public sealed record CreateProjectRepositoryCommand(Guid ProjectId, CreateProjectRepositoryRequest Request)
    : IRequest<ServiceResult<Guid>>;
public sealed record GetProjectRepositoryQuery(Guid ProjectId)
    : IRequest<ServiceResult<ProjectRepositoryDto>>;
public sealed record ListProjectRevisionsQuery(Guid ProjectId, int Page, int PageSize)
    : IRequest<ServiceResult<ProjectRevisionPageDto>>;
public sealed record RegisterProjectWorkspaceCommand(Guid ProjectId, RegisterProjectWorkspaceRequest Request)
    : IRequest<ServiceResult<ProjectWorkspaceDto>>;
public sealed record PullProjectWorkspaceCommand(Guid ProjectId, Guid WorkspaceId)
    : IRequest<ServiceResult<ProjectWorkspaceSnapshotDto>>;
public sealed record PushProjectRevisionCommand(Guid ProjectId, PushProjectRevisionRequest Request)
    : IRequest<ServiceResult<Guid>>;
public sealed record ReviewProjectRevisionCommand(
    Guid ProjectId, Guid RevisionId, ReviewProjectRevisionRequest Request)
    : IRequest<ServiceResult>;
public sealed record PromoteProjectRevisionCommand(
    Guid ProjectId, PromoteProjectRevisionRequest Request)
    : IRequest<ServiceResult>;
public sealed record RedactProjectRevisionCommand(
    Guid ProjectId, Guid RevisionId, RedactProjectRevisionRequest Request)
    : IRequest<ServiceResult>;
public sealed record RevokeProjectWorkspaceCommand(Guid ProjectId, Guid WorkspaceId)
    : IRequest<ServiceResult>;
public sealed record ListRevisionPromotionsQuery(Guid ProjectId, int Page, int PageSize)
    : IRequest<ServiceResult<RevisionPromotionPageDto>>;

public sealed class CreateProjectRepositoryHandler
    : IRequestHandler<CreateProjectRepositoryCommand, ServiceResult<Guid>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public CreateProjectRepositoryHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<Guid>> Handle(CreateProjectRepositoryCommand command, CancellationToken ct)
    {
        if (!ProjectRules.TryEnum(command.Request.StorageProvider, out ProjectStorageProvider? provider) ||
            provider is null)
            return ServiceResult<Guid>.Failure("Nhà cung cấp lưu trữ không hợp lệ.");
        var project = await _db.Projects.SingleOrDefaultAsync(item => item.Id == command.ProjectId, ct);
        if (project is null) return ServiceResult<Guid>.NotFound("Không tìm thấy dự án.");
        var actor = await ProjectRules.GetActiveAccount(_db, _session, ct);
        if (!await ProjectRules.CanManage(_db, actor, project, ct))
            return ServiceResult<Guid>.Forbidden();
        if (project.ProjectStatus != ProjectStatus.InProgress)
            return ServiceResult<Guid>.Failure("Chỉ có thể cấu hình kho cho dự án đang thực hiện.");
        if (await _db.ProjectRepositories.AnyAsync(item => item.ProjectId == project.Id, ct))
            return ServiceResult<Guid>.Failure("Dự án đã có kho tài nguyên.");
        var result = ProjectRepository.Create(project.Id, provider.Value, command.Request.StorageRootRef);
        if (!result.IsSuccess || result.Data is null)
            return ServiceResult<Guid>.Failure(result.Message);
        _db.ProjectRepositories.Add(result.Data);
        await _db.SaveChangesAsync(ct);
        return ServiceResult<Guid>.Success(result.Data.Id, "Đã tạo kho metadata cho dự án.");
    }
}

public sealed class GetProjectRepositoryHandler
    : IRequestHandler<GetProjectRepositoryQuery, ServiceResult<ProjectRepositoryDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public GetProjectRepositoryHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<ProjectRepositoryDto>> Handle(GetProjectRepositoryQuery query, CancellationToken ct)
    {
        var project = await _db.Projects.SingleOrDefaultAsync(item => item.Id == query.ProjectId, ct);
        if (project is null) return ServiceResult<ProjectRepositoryDto>.NotFound("Không tìm thấy dự án.");
        var actor = await ProjectRules.GetActiveAccount(_db, _session, ct);
        if (!await ProjectResourceRules.CanAccessRepository(_db, actor, project, ct))
            return ServiceResult<ProjectRepositoryDto>.Forbidden();
        var repository = await _db.ProjectRepositories.AsNoTracking()
            .Include(item => item.Environments)
            .SingleOrDefaultAsync(item => item.ProjectId == project.Id, ct);
        if (repository is null)
            return ServiceResult<ProjectRepositoryDto>.NotFound("Dự án chưa có kho tài nguyên.");
        var revisions = await _db.ProjectRevisions.AsNoTracking()
            .Include(item => item.Files)
            .Where(item => item.RepositoryId == repository.Id)
            .ToListAsync(ct);
        return ServiceResult<ProjectRepositoryDto>.Success(new ProjectRepositoryDto(
            repository.Id,
            repository.ProjectId,
            repository.StorageProvider.ToString(),
            repository.StorageRootRef,
            repository.GetTotalSize(revisions),
            repository.Environments.Select(item =>
                new ProjectEnvironmentDto(item.Environment.ToString(), item.CurrentRevisionId)).ToArray()));
    }
}

public sealed class ListProjectRevisionsHandler
    : IRequestHandler<ListProjectRevisionsQuery, ServiceResult<ProjectRevisionPageDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public ListProjectRevisionsHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<ProjectRevisionPageDto>> Handle(
        ListProjectRevisionsQuery query, CancellationToken ct)
    {
        if (!ProjectRules.TryPaging(query.Page, query.PageSize, out var skip))
            return ServiceResult<ProjectRevisionPageDto>.Failure("Page hoặc PageSize không hợp lệ.");
        var project = await _db.Projects.SingleOrDefaultAsync(item => item.Id == query.ProjectId, ct);
        if (project is null) return ServiceResult<ProjectRevisionPageDto>.NotFound("Không tìm thấy dự án.");
        var actor = await ProjectRules.GetActiveAccount(_db, _session, ct);
        if (!await ProjectResourceRules.CanAccessRepository(_db, actor, project, ct))
            return ServiceResult<ProjectRevisionPageDto>.Forbidden();
        var repository = await _db.ProjectRepositories.AsNoTracking()
            .SingleOrDefaultAsync(item => item.ProjectId == project.Id, ct);
        if (repository is null)
            return ServiceResult<ProjectRevisionPageDto>.NotFound("Dự án chưa có kho tài nguyên.");
        var revisionsQuery = _db.ProjectRevisions.AsNoTracking()
            .Include(item => item.Files)
            .Include(item => item.Reviews)
            .Where(item => item.RepositoryId == repository.Id)
            .OrderByDescending(item => item.CreatedAtUtc)
            .ThenByDescending(item => item.Id);
        var total = await revisionsQuery.CountAsync(ct);
        var revisions = await revisionsQuery.Skip(skip).Take(query.PageSize).ToListAsync(ct);
        return ServiceResult<ProjectRevisionPageDto>.Success(new ProjectRevisionPageDto(
            revisions.Select(ProjectResourceMappings.ToDto).ToArray(),
            query.Page, query.PageSize, total));
    }
}

public sealed class RegisterProjectWorkspaceHandler
    : IRequestHandler<RegisterProjectWorkspaceCommand, ServiceResult<ProjectWorkspaceDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public RegisterProjectWorkspaceHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<ProjectWorkspaceDto>> Handle(
        RegisterProjectWorkspaceCommand command, CancellationToken ct)
    {
        if (!ProjectRules.TryEnum(command.Request.Environment, out ProjectEnvironmentType? environment) ||
            environment is null)
            return ServiceResult<ProjectWorkspaceDto>.Failure("Môi trường dự án không hợp lệ.");
        var actor = await ProjectRules.GetActiveAccount(_db, _session, ct);
        if (actor is null) return ServiceResult<ProjectWorkspaceDto>.Forbidden();
        var project = await _db.Projects.SingleOrDefaultAsync(item => item.Id == command.ProjectId, ct);
        if (project is null) return ServiceResult<ProjectWorkspaceDto>.NotFound("Không tìm thấy dự án.");
        if (project.ProjectStatus != ProjectStatus.InProgress ||
            await ProjectResourceRules.GetActiveMember(_db, project.Id, actor.Id, ct) is null)
            return ServiceResult<ProjectWorkspaceDto>.Forbidden("Cần là thành viên đang hoạt động của dự án.");
        var repository = await _db.ProjectRepositories.Include(item => item.Environments)
            .SingleOrDefaultAsync(item => item.ProjectId == project.Id, ct);
        if (repository is null)
            return ServiceResult<ProjectWorkspaceDto>.NotFound("Dự án chưa có kho tài nguyên.");
        if (string.IsNullOrWhiteSpace(command.Request.DeviceName) || command.Request.DeviceName.Length > 200)
            return ServiceResult<ProjectWorkspaceDto>.Failure("Tên thiết bị không hợp lệ.");
        var baseRevisionId = repository.GetEnvironment(environment.Value)?.CurrentRevisionId;
        var workspace = await _db.LocalProjectWorkspaces.SingleOrDefaultAsync(item =>
            item.RepositoryId == repository.Id &&
            item.AccountId == actor.Id &&
            item.DeviceName == command.Request.DeviceName.Trim() &&
            item.Environment == environment.Value, ct);
        if (workspace is null)
        {
            var result = LocalProjectWorkspace.Create(
                repository.Id, actor.Id, command.Request.DeviceName, environment.Value, baseRevisionId);
            if (!result.IsSuccess || result.Data is null)
                return ServiceResult<ProjectWorkspaceDto>.Failure(result.Message);
            workspace = result.Data;
            _db.LocalProjectWorkspaces.Add(workspace);
        }
        else
        {
            workspace.Reactivate();
            workspace.Sync(baseRevisionId);
        }
        await _db.SaveChangesAsync(ct);
        return ServiceResult<ProjectWorkspaceDto>.Success(ProjectResourceMappings.ToDto(workspace));
    }
}

public sealed class PullProjectWorkspaceHandler
    : IRequestHandler<PullProjectWorkspaceCommand, ServiceResult<ProjectWorkspaceSnapshotDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public PullProjectWorkspaceHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<ProjectWorkspaceSnapshotDto>> Handle(
        PullProjectWorkspaceCommand command, CancellationToken ct)
    {
        var actor = await ProjectRules.GetActiveAccount(_db, _session, ct);
        if (actor is null) return ServiceResult<ProjectWorkspaceSnapshotDto>.Forbidden();
        var project = await _db.Projects.SingleOrDefaultAsync(item => item.Id == command.ProjectId, ct);
        if (project is null) return ServiceResult<ProjectWorkspaceSnapshotDto>.NotFound("Không tìm thấy dự án.");
        if (await ProjectResourceRules.GetActiveMember(_db, project.Id, actor.Id, ct) is null)
            return ServiceResult<ProjectWorkspaceSnapshotDto>.Forbidden();
        var workspace = await _db.LocalProjectWorkspaces.SingleOrDefaultAsync(item =>
            item.Id == command.WorkspaceId && item.AccountId == actor.Id, ct);
        if (workspace is null || workspace.RepositoryId !=
            await _db.ProjectRepositories.Where(item => item.ProjectId == project.Id)
                .Select(item => item.Id).SingleOrDefaultAsync(ct))
            return ServiceResult<ProjectWorkspaceSnapshotDto>.NotFound("Không tìm thấy vùng làm việc.");
        if (workspace.WorkspaceStatus != LocalWorkspaceStatus.Active)
            return ServiceResult<ProjectWorkspaceSnapshotDto>.Forbidden("Vùng làm việc đã bị thu hồi.");
        var repository = await _db.ProjectRepositories.AsNoTracking()
            .Include(item => item.Environments)
            .SingleAsync(item => item.Id == workspace.RepositoryId, ct);
        var revisionId = repository.GetEnvironment(workspace.Environment)?.CurrentRevisionId;
        ProjectRevision? revision = null;
        if (revisionId.HasValue)
        {
            revision = await _db.ProjectRevisions.AsNoTracking()
                .Include(item => item.Files)
                .SingleOrDefaultAsync(item => item.Id == revisionId.Value, ct);
            if (revision is null)
                return ServiceResult<ProjectWorkspaceSnapshotDto>.Error("Môi trường trỏ tới revision không tồn tại.");
        }
        workspace.Sync(revisionId);
        await _db.SaveChangesAsync(ct);
        return ServiceResult<ProjectWorkspaceSnapshotDto>.Success(new ProjectWorkspaceSnapshotDto(
            ProjectResourceMappings.ToDto(workspace),
            revision?.Id,
            revision?.ManifestHash,
            revision?.IsRedacted ?? false,
            revision is null || revision.IsRedacted
                ? Array.Empty<RevisionFileEntryInput>()
                : revision.Files.Select(ProjectResourceMappings.ToInput).ToArray()));
    }
}

public sealed class PushProjectRevisionHandler
    : IRequestHandler<PushProjectRevisionCommand, ServiceResult<Guid>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public PushProjectRevisionHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<Guid>> Handle(PushProjectRevisionCommand command, CancellationToken ct)
    {
        var actor = await ProjectRules.GetActiveAccount(_db, _session, ct);
        if (actor is null) return ServiceResult<Guid>.Forbidden();
        var project = await _db.Projects.SingleOrDefaultAsync(item => item.Id == command.ProjectId, ct);
        if (project is null) return ServiceResult<Guid>.NotFound("Không tìm thấy dự án.");
        if (project.ProjectStatus != ProjectStatus.InProgress)
            return ServiceResult<Guid>.Failure("Không thể đẩy revision khi dự án không hoạt động.");
        if (await ProjectResourceRules.GetActiveMember(_db, project.Id, actor.Id, ct) is null)
            return ServiceResult<Guid>.Forbidden();
        var workspace = await _db.LocalProjectWorkspaces.SingleOrDefaultAsync(item =>
            item.Id == command.Request.WorkspaceId && item.AccountId == actor.Id, ct);
        if (workspace is null)
            return ServiceResult<Guid>.NotFound("Không tìm thấy vùng làm việc.");
        if (workspace.WorkspaceStatus != LocalWorkspaceStatus.Active ||
            workspace.Environment != ProjectEnvironmentType.Dev)
            return ServiceResult<Guid>.Forbidden("Chỉ vùng làm việc Dev đang hoạt động được phép đẩy revision.");
        var repository = await _db.ProjectRepositories.Include(item => item.Environments)
            .SingleOrDefaultAsync(item => item.Id == workspace.RepositoryId &&
                                           item.ProjectId == project.Id, ct);
        if (repository is null)
            return ServiceResult<Guid>.NotFound("Không tìm thấy kho tài nguyên.");
        var currentDevId = repository.GetEnvironment(ProjectEnvironmentType.Dev)?.CurrentRevisionId;
        if (command.Request.BaseRevisionId != workspace.BaseRevisionId ||
            workspace.BaseRevisionId != currentDevId)
            return ServiceResult<Guid>.Failure("Revision gốc đã cũ; hãy pull phiên bản Dev mới nhất trước khi push.");
        var created = ProjectRevision.Create(
            repository.Id,
            project.Id,
            currentDevId,
            command.Request.Message,
            actor.Id,
            command.Request.Files);
        if (!created.IsSuccess || created.Data is null)
            return ServiceResult<Guid>.Failure(created.Message);
        var update = repository.SetDevRevision(created.Data.Id);
        if (!update.IsSuccess) return ServiceResult<Guid>.Failure(update.Message);
        workspace.Sync(created.Data.Id);
        _db.ProjectRevisions.Add(created.Data);
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ServiceResult<Guid>.Failure(
                "Revision gốc vừa được cập nhật bởi một thao tác khác; hãy pull phiên bản Dev mới nhất.");
        }
        return ServiceResult<Guid>.Success(created.Data.Id, "Đã tạo revision Dev.");
    }
}

public sealed class ReviewProjectRevisionHandler
    : IRequestHandler<ReviewProjectRevisionCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public ReviewProjectRevisionHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(ReviewProjectRevisionCommand command, CancellationToken ct)
    {
        if (!ProjectRules.TryEnum(command.Request.Result, out RevisionReviewResult? reviewResult) ||
            reviewResult is null)
            return ServiceResult.Failure("Kết quả review không hợp lệ.");
        var actor = await ProjectRules.GetActiveAccount(_db, _session, ct);
        if (actor is null) return ServiceResult.Forbidden();
        var project = await _db.Projects.SingleOrDefaultAsync(item => item.Id == command.ProjectId, ct);
        if (project is null) return ServiceResult.NotFound("Không tìm thấy dự án.");
        if (project.ProjectStatus != ProjectStatus.InProgress)
            return ServiceResult.Failure("Chỉ có thể review revision khi dự án đang thực hiện.");
        var member = await ProjectResourceRules.GetActiveMember(_db, project.Id, actor.Id, ct);
        if (member is null) return ServiceResult.Forbidden();
        var revision = await _db.ProjectRevisions.Include(item => item.Reviews)
            .SingleOrDefaultAsync(item => item.Id == command.RevisionId && item.ProjectId == project.Id, ct);
        if (revision is null) return ServiceResult.NotFound("Không tìm thấy revision.");
        var result = revision.Review(actor.Id, member.Role, reviewResult.Value, command.Request.Comment);
        if (!result.IsSuccess) return ServiceResult.Failure(result.Message);
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success("Đã ghi nhận review.");
    }
}

public sealed class PromoteProjectRevisionHandler
    : IRequestHandler<PromoteProjectRevisionCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public PromoteProjectRevisionHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(PromoteProjectRevisionCommand command, CancellationToken ct)
    {
        if (!ProjectRules.TryEnum(command.Request.Environment, out ProjectEnvironmentType? target) ||
            target is null || target == ProjectEnvironmentType.Dev)
            return ServiceResult.Failure("Chỉ có thể thăng cấp revision sang Test hoặc Product.");
        var actor = await ProjectRules.GetActiveAccount(_db, _session, ct);
        if (actor is null) return ServiceResult.Forbidden();
        var project = await _db.Projects.SingleOrDefaultAsync(item => item.Id == command.ProjectId, ct);
        if (project is null) return ServiceResult.NotFound("Không tìm thấy dự án.");
        if (project.ProjectStatus != ProjectStatus.InProgress)
            return ServiceResult.Failure("Chỉ có thể thăng cấp revision khi dự án đang thực hiện.");
        var member = await ProjectResourceRules.GetActiveMember(_db, project.Id, actor.Id, ct);
        if (member is null) return ServiceResult.Forbidden();
        var repository = await _db.ProjectRepositories.Include(item => item.Environments)
            .SingleOrDefaultAsync(item => item.ProjectId == project.Id, ct);
        if (repository is null) return ServiceResult.NotFound("Dự án chưa có kho tài nguyên.");
        var source = target == ProjectEnvironmentType.Test
            ? ProjectEnvironmentType.Dev
            : ProjectEnvironmentType.Test;
        if (repository.GetEnvironment(source)?.CurrentRevisionId != command.Request.RevisionId)
            return ServiceResult.Failure($"Revision phải là phiên bản hiện tại của môi trường {source}.");
        var revision = await _db.ProjectRevisions.Include(item => item.Reviews)
            .SingleOrDefaultAsync(item => item.Id == command.Request.RevisionId &&
                                          item.ProjectId == project.Id, ct);
        if (revision is null) return ServiceResult.NotFound("Không tìm thấy revision.");
        if (revision.IsRedacted) return ServiceResult.Failure("Không thể thăng cấp revision đã bị che.");
        if (target == ProjectEnvironmentType.Test)
        {
            if (member.Role is not (ProjectRole.Owner or ProjectRole.ProjectManager))
                return ServiceResult.Forbidden("Chỉ Owner hoặc Project Manager được thăng cấp Dev sang Test.");
            if (!revision.HasApprovedReview || revision.HasRejectedReview)
                return ServiceResult.Failure("Cần có review được duyệt và không có review bị từ chối trước khi thăng cấp sang Test.");
        }
        else
        {
            if (member.Role is not (ProjectRole.Owner or ProjectRole.TechnicalLeader))
                return ServiceResult.Forbidden("Chỉ Owner hoặc TechnicalLeader được thăng cấp Test sang Product.");
            if (revision.HasRejectedReview ||
                !revision.HasApprovedReviewFrom(ProjectRole.QaQc) ||
                !(revision.HasApprovedReviewFrom(ProjectRole.Owner) ||
                  revision.HasApprovedReviewFrom(ProjectRole.TechnicalLeader)))
                return ServiceResult.Failure("Product cần review duyệt từ QA/QC và Owner hoặc TechnicalLeader, đồng thời không có review bị từ chối.");
        }
        if (repository.GetEnvironment(target.Value)?.CurrentRevisionId == revision.Id)
            return ServiceResult.Failure("Revision này đã ở môi trường đích.");
        var promoted = repository.Promote(revision.Id, target.Value, actor.Id);
        if (!promoted.IsSuccess) return ServiceResult.Failure(promoted.Message);
        var record = RevisionPromotion.Create(repository.Id, revision.Id, actor.Id, target.Value);
        if (!record.IsSuccess || record.Data is null) return ServiceResult.Failure(record.Message);
        _db.RevisionPromotions.Add(record.Data);
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ServiceResult.Failure("Môi trường vừa được cập nhật; hãy tải lại trạng thái kho.");
        }
        return ServiceResult.Success($"Đã thăng cấp revision sang {target}.");
    }
}

public sealed class RedactProjectRevisionHandler
    : IRequestHandler<RedactProjectRevisionCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public RedactProjectRevisionHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(RedactProjectRevisionCommand command, CancellationToken ct)
    {
        var admin = await ProjectRules.GetSystemAdmin(_db, _session, ct);
        if (admin is null) return ServiceResult.Forbidden();
        var revision = await _db.ProjectRevisions.SingleOrDefaultAsync(item =>
            item.Id == command.RevisionId && item.ProjectId == command.ProjectId, ct);
        if (revision is null) return ServiceResult.NotFound("Không tìm thấy revision.");
        var redacted = revision.Redact(admin.Id, command.Request.Reason);
        if (!redacted.IsSuccess) return ServiceResult.Failure(redacted.Message);
        var audit = AuditLog.Record(admin.Id, "ProjectRevisionRedacted", "ProjectRevision", revision.Id);
        if (!audit.IsSuccess || audit.Data is null) return ServiceResult.Error(audit.Message);
        _db.AuditLogs.Add(audit.Data);
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success(
            "Đã ẩn metadata revision trong TaskMind; các đối tượng gốc trên storage ngoài chưa bị xóa.");
    }
}

public sealed class RevokeProjectWorkspaceHandler
    : IRequestHandler<RevokeProjectWorkspaceCommand, ServiceResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public RevokeProjectWorkspaceHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult> Handle(RevokeProjectWorkspaceCommand command, CancellationToken ct)
    {
        var actor = await ProjectRules.GetActiveAccount(_db, _session, ct);
        if (actor is null) return ServiceResult.Forbidden();
        var project = await _db.Projects.SingleOrDefaultAsync(item => item.Id == command.ProjectId, ct);
        if (project is null) return ServiceResult.NotFound("Không tìm thấy dự án.");
        var repositoryId = await _db.ProjectRepositories.Where(item => item.ProjectId == project.Id)
            .Select(item => (Guid?)item.Id).SingleOrDefaultAsync(ct);
        if (!repositoryId.HasValue) return ServiceResult.NotFound("Dự án chưa có kho tài nguyên.");
        var workspace = await _db.LocalProjectWorkspaces.SingleOrDefaultAsync(item =>
            item.Id == command.WorkspaceId && item.RepositoryId == repositoryId.Value, ct);
        if (workspace is null) return ServiceResult.NotFound("Không tìm thấy vùng làm việc.");
        if (workspace.AccountId != actor.Id &&
            !await ProjectRules.CanManage(_db, actor, project, ct))
            return ServiceResult.Forbidden();
        workspace.Revoke();
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Success("Đã thu hồi vùng làm việc.");
    }
}

public sealed class ListRevisionPromotionsHandler
    : IRequestHandler<ListRevisionPromotionsQuery, ServiceResult<RevisionPromotionPageDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentSessionProvider _session;
    public ListRevisionPromotionsHandler(IApplicationDbContext db, ICurrentSessionProvider session)
    {
        _db = db;
        _session = session;
    }

    public async Task<ServiceResult<RevisionPromotionPageDto>> Handle(
        ListRevisionPromotionsQuery query, CancellationToken ct)
    {
        if (!ProjectRules.TryPaging(query.Page, query.PageSize, out var skip))
            return ServiceResult<RevisionPromotionPageDto>.Failure("Page hoặc PageSize không hợp lệ.");
        var project = await _db.Projects.SingleOrDefaultAsync(item => item.Id == query.ProjectId, ct);
        if (project is null)
            return ServiceResult<RevisionPromotionPageDto>.NotFound("Không tìm thấy dự án.");
        var actor = await ProjectRules.GetActiveAccount(_db, _session, ct);
        if (!await ProjectResourceRules.CanAccessRepository(_db, actor, project, ct))
            return ServiceResult<RevisionPromotionPageDto>.Forbidden();
        var repositoryId = await _db.ProjectRepositories.Where(item => item.ProjectId == project.Id)
            .Select(item => (Guid?)item.Id).SingleOrDefaultAsync(ct);
        if (!repositoryId.HasValue)
            return ServiceResult<RevisionPromotionPageDto>.NotFound("Dự án chưa có kho tài nguyên.");
        var recordsQuery = _db.RevisionPromotions.AsNoTracking()
            .Where(item => item.RepositoryId == repositoryId.Value)
            .OrderByDescending(item => item.PromotedAtUtc);
        recordsQuery = recordsQuery.ThenByDescending(item => item.Id);
        var total = await recordsQuery.CountAsync(ct);
        var records = await recordsQuery.Skip(skip).Take(query.PageSize).ToListAsync(ct);
        return ServiceResult<RevisionPromotionPageDto>.Success(new RevisionPromotionPageDto(
            records.Select(item => new RevisionPromotionDto(
                item.RevisionId, item.Environment.ToString(),
                item.ResponsibleAccountId, item.PromotedAtUtc)).ToArray(),
            query.Page, query.PageSize, total));
    }
}

internal static class ProjectResourceRules
{
    public static async Task<bool> CanAccessRepository(
        IApplicationDbContext db, Account? account, Project project, CancellationToken ct)
    {
        if (account is null) return false;
        if (await ProjectRules.CanManage(db, account, project, ct)) return true;
        return await db.ProjectMembers.AsNoTracking().AnyAsync(item =>
            EF.Property<Guid>(item, "ProjectId") == project.Id &&
            item.AccountId == account.Id && item.LeftAt == null, ct);
    }

    public static Task<ProjectMember?> GetActiveMember(
        IApplicationDbContext db, Guid projectId, Guid accountId, CancellationToken ct) =>
        db.ProjectMembers.SingleOrDefaultAsync(item =>
            EF.Property<Guid>(item, "ProjectId") == projectId &&
            item.AccountId == accountId && item.LeftAt == null, ct);

    public static async Task RevokeMemberWorkspacesAsync(
        IApplicationDbContext db, Guid projectId, Guid accountId, CancellationToken ct)
    {
        var repositoryId = await db.ProjectRepositories.Where(item => item.ProjectId == projectId)
            .Select(item => (Guid?)item.Id).SingleOrDefaultAsync(ct);
        if (!repositoryId.HasValue) return;
        var workspaces = await db.LocalProjectWorkspaces.Where(item =>
            item.RepositoryId == repositoryId.Value &&
            item.AccountId == accountId &&
            item.WorkspaceStatus == LocalWorkspaceStatus.Active).ToListAsync(ct);
        foreach (var workspace in workspaces) workspace.Revoke();
    }

    public static async Task RevokeProjectWorkspacesAsync(
        IApplicationDbContext db, Guid projectId, CancellationToken ct)
    {
        var repositoryId = await db.ProjectRepositories.Where(item => item.ProjectId == projectId)
            .Select(item => (Guid?)item.Id).SingleOrDefaultAsync(ct);
        if (!repositoryId.HasValue) return;
        var workspaces = await db.LocalProjectWorkspaces.Where(item =>
            item.RepositoryId == repositoryId.Value &&
            item.WorkspaceStatus == LocalWorkspaceStatus.Active).ToListAsync(ct);
        foreach (var workspace in workspaces) workspace.Revoke();
    }

    public static async Task RevokeAccountWorkspacesAsync(
        IApplicationDbContext db, Guid accountId, CancellationToken ct)
    {
        var projectIds = await db.ProjectMembers.Where(item => item.AccountId == accountId)
            .Select(item => EF.Property<Guid>(item, "ProjectId"))
            .Distinct()
            .ToListAsync(ct);
        if (projectIds.Count == 0) return;
        var repositoryIds = await db.ProjectRepositories
            .Where(item => projectIds.Contains(item.ProjectId))
            .Select(item => item.Id)
            .ToListAsync(ct);
        if (repositoryIds.Count == 0) return;
        var workspaces = await db.LocalProjectWorkspaces.Where(item =>
            repositoryIds.Contains(item.RepositoryId) &&
            item.AccountId == accountId &&
            item.WorkspaceStatus == LocalWorkspaceStatus.Active).ToListAsync(ct);
        foreach (var workspace in workspaces) workspace.Revoke();
    }
}

internal static class ProjectResourceMappings
{
    public static ProjectRevisionDto ToDto(ProjectRevision item) => new(
        item.Id,
        item.ParentRevisionId,
        item.ManifestHash,
        item.Message,
        item.AuthorAccountId,
        item.IsRedacted,
        item.RedactionReason,
        item.CreatedAtUtc,
        item.IsRedacted
            ? Array.Empty<RevisionFileEntryInput>()
            : item.Files.Select(ToInput).ToArray(),
        item.Reviews.Select(review => new RevisionReviewDto(
            review.ReviewerAccountId,
            review.ReviewerRole.ToString(),
            review.Result.ToString(),
            review.Comment,
            review.ReviewedAtUtc)).ToArray());

    public static ProjectWorkspaceDto ToDto(LocalProjectWorkspace item) => new(
        item.Id,
        item.DeviceName,
        item.Environment.ToString(),
        item.BaseRevisionId,
        item.WorkspaceStatus.ToString(),
        item.LastSyncedAtUtc);

    public static RevisionFileEntryInput ToInput(RevisionFileEntry item) =>
        new(item.Path, item.StorageObjectRef, item.ContentHash, item.Size);
}
