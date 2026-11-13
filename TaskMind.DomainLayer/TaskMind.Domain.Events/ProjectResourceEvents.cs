using TaskMind.DomainLayer.TaskMind.Domain.Commons.Events;
using TaskMind.DomainLayer.TaskMind.Domain.Enums;

namespace TaskMind.DomainLayer.TaskMind.Domain.Events;

public sealed class RevisionCreatedEvent : DomainEvent
{
    public Guid RepositoryId { get; init; }
    public Guid RevisionId { get; init; }
    public Guid ProjectId { get; init; }
    public Guid AuthorAccountId { get; init; }
}

public sealed class RevisionReviewedEvent : DomainEvent
{
    public Guid RevisionId { get; init; }
    public Guid ReviewerAccountId { get; init; }
    public RevisionReviewResult Result { get; init; }
}

public sealed class RevisionPromotedEvent : DomainEvent
{
    public Guid RepositoryId { get; init; }
    public Guid RevisionId { get; init; }
    public Guid ResponsibleAccountId { get; init; }
    public ProjectEnvironmentType Environment { get; init; }
}

public sealed class LocalWorkspaceRegisteredEvent : DomainEvent
{
    public Guid WorkspaceId { get; init; }
    public Guid RepositoryId { get; init; }
    public Guid AccountId { get; init; }
}

public sealed class LocalWorkspaceRevokedEvent : DomainEvent
{
    public Guid WorkspaceId { get; init; }
    public Guid RepositoryId { get; init; }
    public Guid AccountId { get; init; }
}
