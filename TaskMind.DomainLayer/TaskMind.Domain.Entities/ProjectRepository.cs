using Microsoft.EntityFrameworkCore;
using TaskMind.DomainLayer.TaskMind.Domain.Commons.Cores;
using TaskMind.DomainLayer.TaskMind.Domain.Commons.Result;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;
using TaskMind.DomainLayer.TaskMind.Domain.Events;

namespace TaskMind.DomainLayer.TaskMind.Domain.Entities;

[Index(nameof(ProjectId), IsUnique = true)]
public sealed class ProjectRepository : AggregateRoot
{
    public Guid ProjectId { get; private set; }
    public ProjectStorageProvider StorageProvider { get; private set; }
    public string StorageRootRef { get; private set; } = string.Empty;
    private readonly List<ProjectEnvironment> _environments = new();
    public IReadOnlyCollection<ProjectEnvironment> Environments => _environments.AsReadOnly();

    private ProjectRepository() { }

    private ProjectRepository(Guid projectId, ProjectStorageProvider provider, string rootRef)
    {
        ProjectId = projectId;
        StorageProvider = provider;
        StorageRootRef = rootRef.Trim();
        foreach (var environment in Enum.GetValues<ProjectEnvironmentType>())
            _environments.Add(ProjectEnvironment.Create(environment));
    }

    public static Result<ProjectRepository> Create(
        Guid projectId, ProjectStorageProvider provider, string storageRootRef)
    {
        if (projectId == Guid.Empty)
            return Result<ProjectRepository>.Failure("ProjectId không hợp lệ.");
        if (!Enum.IsDefined(provider))
            return Result<ProjectRepository>.Failure("Nhà cung cấp lưu trữ không được hỗ trợ.");
        if (string.IsNullOrWhiteSpace(storageRootRef) || storageRootRef.Length > 2048 ||
            storageRootRef.Any(char.IsControl))
            return Result<ProjectRepository>.Failure("StorageRootRef không hợp lệ.");
        return Result<ProjectRepository>.Success(
            new ProjectRepository(projectId, provider, storageRootRef));
    }

    public ProjectEnvironment? GetEnvironment(ProjectEnvironmentType environment) =>
        _environments.SingleOrDefault(item => item.Environment == environment);

    public Result Promote(Guid revisionId, ProjectEnvironmentType target, Guid responsibleAccountId)
    {
        if (revisionId == Guid.Empty || responsibleAccountId == Guid.Empty)
            return Result.Failure("RevisionId hoặc người chịu trách nhiệm không hợp lệ.");
        var current = GetEnvironment(target);
        if (current is null) return Result.Failure("Không tìm thấy môi trường dự án.");
        current.SetCurrentRevision(revisionId);
        AddDomainEvent(new RevisionPromotedEvent
        {
            RepositoryId = Id,
            RevisionId = revisionId,
            ResponsibleAccountId = responsibleAccountId,
            Environment = target
        });
        return Result.Success();
    }

    public Result SetDevRevision(Guid revisionId)
    {
        if (revisionId == Guid.Empty) return Result.Failure("RevisionId không hợp lệ.");
        var dev = GetEnvironment(ProjectEnvironmentType.Dev);
        if (dev is null) return Result.Failure("Không tìm thấy môi trường Dev.");
        dev.SetCurrentRevision(revisionId);
        return Result.Success();
    }

    public long GetTotalSize(IEnumerable<ProjectRevision> revisions) =>
        revisions.Where(item => item.RepositoryId == Id && !item.IsRedacted)
            .SelectMany(item => item.Files)
            .Sum(item => item.Size);

}

public sealed class ProjectEnvironment
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public ProjectEnvironmentType Environment { get; private set; }
    public Guid? CurrentRevisionId { get; private set; }

    private ProjectEnvironment() { }
    private ProjectEnvironment(ProjectEnvironmentType environment) => Environment = environment;

    internal static ProjectEnvironment Create(ProjectEnvironmentType environment) => new(environment);

    internal void SetCurrentRevision(Guid revisionId)
    {
        if (revisionId == Guid.Empty) throw new ArgumentException("RevisionId không hợp lệ.", nameof(revisionId));
        CurrentRevisionId = revisionId;
    }
}

[Index(nameof(RepositoryId), nameof(CreatedAtUtc))]
public sealed class ProjectRevision : AggregateRoot
{
    public Guid RepositoryId { get; private set; }
    public Guid ProjectId { get; private set; }
    public Guid? ParentRevisionId { get; private set; }
    public string ManifestHash { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public Guid AuthorAccountId { get; private set; }
    public bool IsRedacted { get; private set; }
    public string? RedactionReason { get; private set; }
    public Guid? RedactedByAccountId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;

    private readonly List<RevisionFileEntry> _files = new();
    public IReadOnlyCollection<RevisionFileEntry> Files => _files.AsReadOnly();
    private readonly List<RevisionReview> _reviews = new();
    public IReadOnlyCollection<RevisionReview> Reviews => _reviews.AsReadOnly();

    private ProjectRevision() { }

    private ProjectRevision(
        Guid repositoryId, Guid projectId, Guid? parentRevisionId, string manifestHash,
        string message, Guid authorAccountId, IEnumerable<RevisionFileEntry> files)
    {
        RepositoryId = repositoryId;
        ProjectId = projectId;
        ParentRevisionId = parentRevisionId;
        ManifestHash = manifestHash;
        Message = message.Trim();
        AuthorAccountId = authorAccountId;
        _files.AddRange(files);
    }

    public static Result<ProjectRevision> Create(
        Guid repositoryId,
        Guid projectId,
        Guid? parentRevisionId,
        string message,
        Guid authorAccountId,
        IReadOnlyCollection<RevisionFileEntryInput> files)
    {
        if (repositoryId == Guid.Empty || projectId == Guid.Empty || authorAccountId == Guid.Empty)
            return Result<ProjectRevision>.Failure("RepositoryId, ProjectId hoặc AuthorAccountId không hợp lệ.");
        if (parentRevisionId == Guid.Empty)
            return Result<ProjectRevision>.Failure("ParentRevisionId không hợp lệ.");
        if (string.IsNullOrWhiteSpace(message) || message.Length > 500 || message.Contains('\0'))
            return Result<ProjectRevision>.Failure("Thông điệp revision bắt buộc và tối đa 500 ký tự.");
        if (files is null || files.Count == 0 || files.Count > 10_000)
            return Result<ProjectRevision>.Failure("Snapshot phải có từ 1 đến 10.000 file.");
        if (files.Any(item => !RevisionFileEntry.IsValid(item)) ||
            files.Select(item => item.Path.Trim().Replace('\\', '/'))
                .Distinct(StringComparer.OrdinalIgnoreCase).Count() != files.Count)
            return Result<ProjectRevision>.Failure("Danh sách file không hợp lệ hoặc có đường dẫn trùng.");

        var entries = files.Select(item => RevisionFileEntry.Create(item)).ToArray();
        var manifest = string.Join('\n', entries
            .OrderBy(item => item.Path, StringComparer.Ordinal)
            .Select(item => $"{item.Path}\0{item.StorageObjectRef}\0{item.ContentHash}\0{item.Size}"));
        var hash = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(manifest)));
        return Result<ProjectRevision>.Success(
            CreateWithEvent(repositoryId, projectId, parentRevisionId, hash, message, authorAccountId, entries));
    }

    private static ProjectRevision CreateWithEvent(
        Guid repositoryId, Guid projectId, Guid? parentRevisionId,
        string manifestHash, string message, Guid authorAccountId,
        IEnumerable<RevisionFileEntry> files)
    {
        var revision = new ProjectRevision(
            repositoryId, projectId, parentRevisionId, manifestHash, message, authorAccountId, files);
        revision.AddDomainEvent(new RevisionCreatedEvent
        {
            RepositoryId = repositoryId,
            RevisionId = revision.Id,
            ProjectId = projectId,
            AuthorAccountId = authorAccountId
        });
        return revision;
    }

    public Result Review(Guid reviewerAccountId, ProjectRole role, RevisionReviewResult result, string? comment)
    {
        if (reviewerAccountId == Guid.Empty || reviewerAccountId == AuthorAccountId)
            return Result.Failure("Người viết không thể tự review revision của mình.");
        if (!Enum.IsDefined(result))
            return Result.Failure("Kết quả review không hợp lệ.");
        if (role is not (ProjectRole.Owner or ProjectRole.TechnicalLeader or ProjectRole.QaQc))
            return Result.Failure("Chỉ Owner, TechnicalLeader hoặc QaQc được review revision.");
        if (comment?.Length > 2000)
            return Result.Failure("Nội dung review tối đa 2.000 ký tự.");
        if (_reviews.Any(item => item.ReviewerAccountId == reviewerAccountId))
            return Result.Failure("Thành viên đã review revision này.");
        var review = RevisionReview.Create(Id, reviewerAccountId, role, result, comment);
        if (!review.IsSuccess || review.Data is null) return Result.Failure(review.Message);
        _reviews.Add(review.Data);
        AddDomainEvent(new RevisionReviewedEvent
        {
            RevisionId = Id,
            ReviewerAccountId = reviewerAccountId,
            Result = result
        });
        return Result.Success();
    }

    public bool HasApprovedReviewFrom(ProjectRole role) =>
        _reviews.Any(item => item.Result == RevisionReviewResult.Approved && item.ReviewerRole == role);

    public bool HasApprovedReview => _reviews.Any(item => item.Result == RevisionReviewResult.Approved);
    public bool HasRejectedReview => _reviews.Any(item => item.Result == RevisionReviewResult.Rejected);

    public Result Redact(Guid adminAccountId, string reason)
    {
        if (IsRedacted) return Result.Failure("Revision đã được che nội dung trước đó.");
        if (adminAccountId == Guid.Empty || string.IsNullOrWhiteSpace(reason) || reason.Length > 2000)
            return Result.Failure("Cần xác định Admin và lý do che nội dung.");
        IsRedacted = true;
        RedactedByAccountId = adminAccountId;
        RedactionReason = reason.Trim();
        return Result.Success();
    }
}

public sealed record RevisionFileEntryInput(
    string Path, string StorageObjectRef, string ContentHash, long Size);

public sealed class RevisionFileEntry
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Path { get; private set; } = string.Empty;
    public string StorageObjectRef { get; private set; } = string.Empty;
    public string ContentHash { get; private set; } = string.Empty;
    public long Size { get; private set; }

    private RevisionFileEntry() { }

    private RevisionFileEntry(string path, string storageObjectRef, string contentHash, long size)
    {
        Path = path.Trim().Replace('\\', '/');
        StorageObjectRef = storageObjectRef.Trim();
        ContentHash = contentHash.ToUpperInvariant();
        Size = size;
    }

    internal static bool IsValid(RevisionFileEntryInput item)
    {
        if (item is null || string.IsNullOrWhiteSpace(item.StorageObjectRef) ||
            item.StorageObjectRef.Length > 2048 ||
            item.StorageObjectRef.Any(char.IsControl) ||
            string.IsNullOrWhiteSpace(item.Path) || item.Path.Length > 1024 ||
            item.Path.Any(char.IsControl) ||
            item.Path.StartsWith('/') || item.Path.Contains(':') ||
            item.Path.Split(['/', '\\']).Any(part => part is "" or "." or ".."))
            return false;
        if (item.Size < 0 || item.Size > 1_000_000_000_000)
            return false;
        return item.ContentHash is { Length: 64 } && item.ContentHash.All(Uri.IsHexDigit);
    }

    internal static RevisionFileEntry Create(RevisionFileEntryInput item) =>
        new(item.Path, item.StorageObjectRef, item.ContentHash, item.Size);
}

public sealed class RevisionReview
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid RevisionId { get; private set; }
    public Guid ReviewerAccountId { get; private set; }
    public ProjectRole ReviewerRole { get; private set; }
    public RevisionReviewResult Result { get; private set; }
    public string Comment { get; private set; } = string.Empty;
    public DateTimeOffset ReviewedAtUtc { get; private set; } = DateTimeOffset.UtcNow;

    private RevisionReview() { }

    private RevisionReview(
        Guid revisionId, Guid reviewerAccountId, ProjectRole role,
        RevisionReviewResult result, string? comment)
    {
        RevisionId = revisionId;
        ReviewerAccountId = reviewerAccountId;
        ReviewerRole = role;
        Result = result;
        Comment = comment?.Trim() ?? string.Empty;
    }

    internal static Result<RevisionReview> Create(
        Guid revisionId, Guid reviewerAccountId, ProjectRole role,
        RevisionReviewResult result, string? comment) =>
        revisionId == Guid.Empty
            ? Result<RevisionReview>.Failure("RevisionId không hợp lệ.")
            : Result<RevisionReview>.Success(new RevisionReview(
                revisionId, reviewerAccountId, role, result, comment));
}

[Index(nameof(RepositoryId), nameof(AccountId), nameof(DeviceName), nameof(Environment), IsUnique = true)]
public sealed class LocalProjectWorkspace : AggregateRoot
{
    public Guid RepositoryId { get; private set; }
    public Guid AccountId { get; private set; }
    public string DeviceName { get; private set; } = string.Empty;
    public ProjectEnvironmentType Environment { get; private set; }
    public Guid? BaseRevisionId { get; private set; }
    public LocalWorkspaceStatus WorkspaceStatus { get; private set; }
    public DateTimeOffset LastSyncedAtUtc { get; private set; }

    private LocalProjectWorkspace() { }

    private LocalProjectWorkspace(
        Guid repositoryId, Guid accountId, string deviceName,
        ProjectEnvironmentType environment, Guid? baseRevisionId)
    {
        RepositoryId = repositoryId;
        AccountId = accountId;
        DeviceName = deviceName.Trim();
        Environment = environment;
        BaseRevisionId = baseRevisionId;
        WorkspaceStatus = LocalWorkspaceStatus.Active;
        LastSyncedAtUtc = DateTimeOffset.UtcNow;
        AddDomainEvent(new LocalWorkspaceRegisteredEvent
        {
            WorkspaceId = Id,
            RepositoryId = repositoryId,
            AccountId = accountId
        });
    }

    public static Result<LocalProjectWorkspace> Create(
        Guid repositoryId, Guid accountId, string deviceName,
        ProjectEnvironmentType environment, Guid? baseRevisionId)
    {
        if (repositoryId == Guid.Empty || accountId == Guid.Empty || string.IsNullOrWhiteSpace(deviceName) ||
            deviceName.Length > 200 || deviceName.Any(char.IsControl) ||
            !Enum.IsDefined(environment) || baseRevisionId == Guid.Empty)
            return Result<LocalProjectWorkspace>.Failure("Thông tin vùng làm việc không hợp lệ.");
        return Result<LocalProjectWorkspace>.Success(
            new LocalProjectWorkspace(repositoryId, accountId, deviceName, environment, baseRevisionId));
    }

    public Result Sync(Guid? revisionId)
    {
        if (WorkspaceStatus != LocalWorkspaceStatus.Active)
            return Result.Failure("Vùng làm việc đã bị thu hồi.");
        if (revisionId == Guid.Empty) return Result.Failure("RevisionId không hợp lệ.");
        BaseRevisionId = revisionId;
        LastSyncedAtUtc = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result Revoke()
    {
        if (WorkspaceStatus == LocalWorkspaceStatus.Revoked) return Result.Success();
        WorkspaceStatus = LocalWorkspaceStatus.Revoked;
        AddDomainEvent(new LocalWorkspaceRevokedEvent
        {
            WorkspaceId = Id,
            RepositoryId = RepositoryId,
            AccountId = AccountId
        });
        return Result.Success();
    }

    public Result Reactivate()
    {
        WorkspaceStatus = LocalWorkspaceStatus.Active;
        LastSyncedAtUtc = DateTimeOffset.UtcNow;
        AddDomainEvent(new LocalWorkspaceRegisteredEvent
        {
            WorkspaceId = Id,
            RepositoryId = RepositoryId,
            AccountId = AccountId
        });
        return Result.Success();
    }
}

public sealed class RevisionPromotion : AggregateRoot
{
    public Guid RepositoryId { get; private set; }
    public Guid RevisionId { get; private set; }
    public Guid ResponsibleAccountId { get; private set; }
    public ProjectEnvironmentType Environment { get; private set; }
    public DateTimeOffset PromotedAtUtc { get; private set; } = DateTimeOffset.UtcNow;

    private RevisionPromotion() { }

    private RevisionPromotion(Guid repositoryId, Guid revisionId, Guid responsibleAccountId, ProjectEnvironmentType environment)
    {
        RepositoryId = repositoryId;
        RevisionId = revisionId;
        ResponsibleAccountId = responsibleAccountId;
        Environment = environment;
    }

    public static Result<RevisionPromotion> Create(
        Guid repositoryId, Guid revisionId, Guid responsibleAccountId, ProjectEnvironmentType environment)
    {
        if (repositoryId == Guid.Empty || revisionId == Guid.Empty || responsibleAccountId == Guid.Empty ||
            !Enum.IsDefined(environment))
            return Result<RevisionPromotion>.Failure("Thông tin thăng cấp revision không hợp lệ.");
        return Result<RevisionPromotion>.Success(
            new RevisionPromotion(repositoryId, revisionId, responsibleAccountId, environment));
    }
}
