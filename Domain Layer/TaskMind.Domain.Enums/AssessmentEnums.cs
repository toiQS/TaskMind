namespace TaskMind.Domain.Enums
{
    /// <summary>Chủ sở hữu bài kiểm tra: công ty hay cơ sở đào tạo (mục 4.6, 4.11).</summary>
    public enum TestOwnerType
    {
        Company,
        School
    }

    /// <summary>
    /// Phương thức nâng cấp độ kỹ năng (mục 4.3.1): thông qua bảo lãnh/xác nhận (Endorsement)
    /// hoặc trải qua chu trình đánh giá năng lực chuẩn của hệ thống (Assessment).
    /// </summary>
    public enum SkillLevelUpMethod
    {
        Endorsement,
        Assessment
    }
}
