using System.Collections.ObjectModel;

namespace TaskMind.WPFs.Modules.Admins.Models
{
    /// <summary>Một user đang sở hữu kỹ năng này, dùng cho danh sách "Người dùng tiêu biểu".</summary>
    public class SkillUserItem
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public SkillLevel Level { get; set; }
        public int EndorsementCount { get; set; }
    }

    /// <summary>Tỉ trọng nguồn sử dụng kỹ năng: User cá nhân / Công ty / Cơ sở đào tạo.</summary>
    public class SkillUsageBySourceItem
    {
        public string SourceLabel { get; set; }
        public int Count { get; set; }

        /// <summary>0 - 100</summary>
        public double Percentage { get; set; }
    }

    /// <summary>
    /// Dữ liệu chi tiết một kỹ năng, dùng cho DetailSkillView.
    /// Gộp: thông tin kỹ năng, số liệu sử dụng, người dùng tiêu biểu,
    /// kỹ năng liên quan, biểu đồ tăng trưởng, lịch sử duyệt/đề xuất.
    /// </summary>
    public class DetailSkillModel
    {
        public SkillModel Skill { get; set; } = new SkillModel();

        public int TotalProjectsRequiring { get; set; }
        public int TotalEndorsements { get; set; }

        public ObservableCollection<SkillUsageBySourceItem> UsageBySource { get; set; } = new ObservableCollection<SkillUsageBySourceItem>();
        public ObservableCollection<SkillUserItem> TopUsers { get; set; } = new ObservableCollection<SkillUserItem>();
        public ObservableCollection<SkillModel> RelatedSkills { get; set; } = new ObservableCollection<SkillModel>();
        public ObservableCollection<ChartPoint> GrowthChart { get; set; } = new ObservableCollection<ChartPoint>();

        /// <summary>Lịch sử duyệt/đề xuất/chỉnh sửa kỹ năng (liên kết mục 5.7 - Audit Log).</summary>
        public ObservableCollection<AuditLogEntryModel> ApprovalHistory { get; set; } = new ObservableCollection<AuditLogEntryModel>();
    }
}