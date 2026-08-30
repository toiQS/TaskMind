namespace TaskMind.Domain.Enums
{
    /// <summary>Trạng thái tin tuyển dụng (mục 4.18).</summary>
    public enum JobPostingStatus
    {
        Draft,
        Open,
        Closed,
        Cancelled
    }

    /// <summary>Trạng thái hồ sơ ứng tuyển (mục 4.18).</summary>
    public enum ApplicationStatus
    {
        Submitted,
        UnderReview,
        Shortlisted,
        Rejected,
        Accepted
    }
}
