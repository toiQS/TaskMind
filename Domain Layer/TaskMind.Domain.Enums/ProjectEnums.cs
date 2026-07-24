namespace TaskMind.Domain.Enums
{
    /// <summary>Vai trò trong phạm vi một dự án (mục 3 - Vai trò chi tiết trong dự án).</summary>
    public enum ProjectRole
    {
        Owner,
        TechnicalLeader,
        ProjectManager,
        QaQc,
        Developer,
        Intern
    }

    public enum ProjectStatus
    {
        InProgress,
        Paused,
        Completed,
        Cancelled
    }

    /// <summary>Nguồn gốc dự án: trực thuộc công ty, cơ sở đào tạo, hay mã nguồn mở (mục 4.7/4.11/4.12).</summary>
    public enum ProjectSourceType
    {
        Company,
        School,
        OpenSource
    }
}