using System;

namespace TaskMind.WPFs.Modules.Companies.Models
{
    /// <summary>
    /// Một chỉ số KPI hiển thị trên thẻ tổng quan (VD: "Dự án đang thực hiện": 4, tăng 12%).
    /// Tái sử dụng TrendDirection đã khai báo ở FindModel.cs để đồng bộ ý nghĩa tăng/giảm/ổn định.
    /// </summary>
    public class QuickStatModel
    {
        public string Title { get; set; }
        public string Value { get; set; }
        public string SubText { get; set; }

        /// <summary>Tên SymbolRegular của WPF-UI (VD: "Board24", "People24") — bind động qua StringToSymbolConverter.</summary>
        public string Icon { get; set; } = "Board24";

        /// <summary>Màu accent riêng cho từng thẻ (hex), bind qua HexToBrushConverter.</summary>
        public string AccentColor { get; set; } = "#4C9AFF";

        public TrendDirection Trend { get; set; } = TrendDirection.Stable;
        public double? TrendPercent { get; set; }
    }

    /// <summary>Tóm tắt tiến độ 1 dự án dùng cho danh sách rút gọn trên Dashboard (khác ProjectModel đầy đủ ở ProjectVM).</summary>
    public class ProjectProgressSummary
    {
        public Guid ProjectId { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public double Progress { get; set; }
        public ProjectStatus Status { get; set; } = ProjectStatus.InProgress;
        public int TaskDone { get; set; }
        public int TaskTotal { get; set; }
    }

    /// <summary>Một mục trong nhật ký hoạt động gần đây (dự án, tuyển dụng, hỗ trợ, nhân sự...).</summary>
    public class RecentActivityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Time { get; set; } = DateTime.Now;
        public string IconSymbol { get; set; } = "Info24";
        public string AccentColor { get; set; } = "#4C9AFF";
    }

    /// <summary>Ứng viên nổi bật cần chú ý (mức khớp kỹ năng cao / đang chờ xử lý), rút gọn từ CandidateModel.</summary>
    public class TopCandidateSummary
    {
        public Guid CandidateId { get; set; } = Guid.NewGuid();
        public string FullName { get; set; }
        public string JobTitle { get; set; }
        public double MatchScore { get; set; }
    }
}