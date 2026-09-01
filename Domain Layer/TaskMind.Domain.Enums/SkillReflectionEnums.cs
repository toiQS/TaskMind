namespace TaskMind.Domain.Enums
{
    /// <summary>Loại phản ánh kỹ năng do công ty chủ động đề xuất (mục 4.3.2). [MỚI - v2.1]</summary>
    public enum SkillReflectionType
    {
        Up,
        Down,
        Add
    }

    /// <summary>Trạng thái xử lý một đề xuất phản ánh kỹ năng (mục 4.3.2). [MỚI - v2.1]</summary>
    public enum SkillReflectionStatus
    {
        /// <summary>Chỉ áp dụng cho Down: chờ Admin hệ thống xem xét có chấp nhận xử lý hay không.</summary>
        PendingAdminReview,

        /// <summary>Chờ nhân sự hoàn thành bài kiểm tra xác minh (TestPaper/Submission).</summary>
        PendingVerification,

        Applied,
        Rejected
    }

    /// <summary>Nguồn gốc phát sinh một thay đổi kỹ năng, dùng cho lịch sử kỹ năng (mục 4.3.3). [MỚI - v2.1]</summary>
    public enum SkillChangeSource
    {
        UserInitiated,
        CompanyReflection
    }

    /// <summary>Kết quả xử lý cuối cùng của một mục lịch sử kỹ năng (mục 4.3.3). [MỚI - v2.1]</summary>
    public enum SkillHistoryOutcome
    {
        Applied,
        Rejected
    }
}
