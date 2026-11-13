namespace TaskMind.DomainLayer.TaskMind.Domain.Enums;

public enum ProjectStorageProvider
{
    GoogleDrive
}

public enum ProjectEnvironmentType
{
    Dev,
    Test,
    Product
}

public enum RevisionReviewResult
{
    Approved,
    Rejected
}

public enum LocalWorkspaceStatus
{
    Active,
    Revoked
}
