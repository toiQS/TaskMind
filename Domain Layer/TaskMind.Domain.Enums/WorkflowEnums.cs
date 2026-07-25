namespace TaskMind.Domain.Enums
{
    public enum SkillLevelUpRequestStatus
    {
        PendingEndorsement,
        PendingAssessment,
        Approved,
        Rejected
    }

    public enum InvoiceStatus
    {
        Pending,
        Issued,
        Paid,
        Overdue
    }

    public enum ExchangeStatus
    {
        Negotiating,
        Active,
        Completed,
        Disputed,
        Cancelled
    }

    /// <summary>Loại thông báo hệ thống ở tầng Domain (mục 5.3), độc lập với NotificationType trong WPF layer.</summary>
    public enum NotificationType
    {
        System,
        Approval,
        Warning,
        Success
    }
}