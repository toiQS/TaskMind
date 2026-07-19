using System;
using System.Collections.Generic;

namespace TaskMind.WPFs.Modules.Companies.Models
{
    /// <summary>Xu hướng tăng/giảm nhu cầu tuyển dụng theo từng kỹ năng.</summary>
    public enum TrendDirection { Up, Down, Stable }

    /// <summary>Trạng thái sẵn sàng nhận việc của ứng viên tự do.</summary>
    public enum FreelancerAvailability { Available, Busy, Unavailable }

    /// <summary>Thống kê xu hướng nhu cầu kỹ năng hiện tại trên toàn hệ thống (mục 5.6).</summary>
    public class SkillTrendModel
    {
        public string SkillName { get; set; }
        public int DemandCount { get; set; }
        public double ChangePercent { get; set; }
        public TrendDirection Direction { get; set; } = TrendDirection.Stable;
    }

    /// <summary>Dự án của công ty đang cần tìm thêm ứng viên tự do tham gia.</summary>
    public class ProjectNeedModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ProjectName { get; set; }
        public List<string> RequiredSkills { get; set; } = new();
        public int OpenSlots { get; set; }
        public bool IsSelected { get; set; }
        public string SkillsDisplay => RequiredSkills is { Count: > 0 } ? string.Join(" · ", RequiredSkills) : "Chưa xác định";
    }

    /// <summary>
    /// Ứng viên tự do (freelancer/người tìm việc/OSS contributor) có thể tìm kiếm và mời tham gia dự án (mục 5.6).
    /// MatchScore tính theo mức khớp kỹ năng cá nhân với dự án đang chọn.
    /// </summary>
    public class FreelanceCandidateModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string FullName { get; set; }
        public string Headline { get; set; }
        public List<string> Skills { get; set; } = new();

        public int ExperienceYears { get; set; }
        public double Rating { get; set; }
        public int CompletedProjects { get; set; }
        public FreelancerAvailability Availability { get; set; } = FreelancerAvailability.Available;
        public string Location { get; set; }
        public string PortfolioUrl { get; set; }

        /// <summary>Tính lại mỗi khi đổi dự án đang chọn (xem RecalculateMatchScores trong VM).</summary>
        public double MatchScore { get; set; }

        public string Initial => string.IsNullOrWhiteSpace(FullName) ? "?" : FullName.Trim()[0].ToString().ToUpper();
        public string SkillsDisplay => Skills is { Count: > 0 } ? string.Join(" · ", Skills) : "Chưa cập nhật kỹ năng";
    }
}