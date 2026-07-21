using System;
using System.Collections.Generic;
using System.Linq;

namespace TaskMind.WPFs.Modules.Staffs.Models
{
    /// <summary>Trạng thái nhân sự — đồng bộ ý nghĩa với StaffStatus bên Modules.Companies (mục 4.5),
    /// do Admin company quản lý; nhân sự chỉ xem trên hồ sơ của chính mình.</summary>
    public enum StaffStatus
    {
        Active,     // Đang làm việc
        Suspended,  // Tạm ngưng
        Resigned    // Đã nghỉ việc
    }

    /// <summary>Mức độ thành thạo kỹ năng cá nhân (mục 4.3).</summary>
    public enum SkillLevel
    {
        Basic,
        Intermediate,
        Proficient,
        Expert
    }

    /// <summary>Quyền riêng tư hồ sơ cá nhân — công khai/ẩn một phần thông tin (mục 4.2).</summary>
    public enum ProfileVisibility
    {
        Public,       // Công khai toàn bộ
        CompanyOnly,  // Chỉ công ty/đồng nghiệp xem được
        Private       // Ẩn, chỉ bản thân xem được
    }

    /// <summary>Nền tảng liên kết mạng xã hội/portfolio cá nhân (mục 4.2).</summary>
    public enum SocialPlatform
    {
        GitHub,
        GitLab,
        LinkedIn,
        Website
    }

    /// <summary>Một liên kết mạng xã hội/portfolio.</summary>
    public class SocialLinkModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public SocialPlatform Platform { get; set; }
        public string Url { get; set; }
    }

    /// <summary>Lịch sử học vấn (mục 4.2).</summary>
    public class EducationHistoryModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string SchoolName { get; set; }
        public string Major { get; set; }
        public int StartYear { get; set; } = DateTime.Now.Year;
        public int? EndYear { get; set; }

        public bool IsOngoing => EndYear == null;
        public string DurationDisplay => IsOngoing ? $"{StartYear} - hiện tại" : $"{StartYear} - {EndYear}";
    }

    /// <summary>Lịch sử làm việc/kinh nghiệm trước đó (mục 4.2). Không bao gồm quá trình làm việc tại
    /// chính công ty hiện tại — phần đó lấy trực tiếp từ Position/Department/JoinDate bên dưới.</summary>
    public class WorkExperienceModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string CompanyName { get; set; }
        public string Position { get; set; }
        public DateTime? StartDate { get; set; } = DateTime.Now;
        public DateTime? EndDate { get; set; }
        public string Description { get; set; }

        public bool IsOngoing => StartDate != null && EndDate == null;
        public string DurationDisplay => StartDate == null
            ? string.Empty
            : $"{StartDate:MM/yyyy} - {(EndDate.HasValue ? EndDate.Value.ToString("MM/yyyy") : "hiện tại")}";
    }

    /// <summary>Một kỹ năng cá nhân + mức độ thành thạo + xác nhận (endorsement) từ đồng đội,
    /// Technical leader hoặc giảng viên (mục 4.3).</summary>
    public class SkillItemModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public SkillLevel Level { get; set; } = SkillLevel.Basic;

        public List<string> EndorsedBy { get; set; } = new();

        public int EndorsementCount => EndorsedBy?.Count ?? 0;
        public bool HasEndorsement => EndorsementCount > 0;
    }

    /// <summary>Một dòng lịch sử tham gia dự án, tự động tổng hợp từ hệ thống (mục 4.2) — chỉ đọc,
    /// không chỉnh sửa trực tiếp trên hồ sơ vì dữ liệu do module Project sinh ra.</summary>
    public class ProjectHistorySummary
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ProjectName { get; set; }
        public string RoleName { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime? EndDate { get; set; }
        public bool IsCompleted { get; set; }

        public string DurationDisplay => $"{StartDate:MM/yyyy} - {(EndDate.HasValue ? EndDate.Value.ToString("MM/yyyy") : "hiện tại")}";
    }

    /// <summary>
    /// Hồ sơ cá nhân của nhân sự trực thuộc công ty, hiển thị trên giao diện riêng của nhân sự
    /// (module Staffs). Các trường Position/Department/Status/JoinDate/LeftDate được đồng bộ một
    /// chiều từ StaffModel bên Modules.Companies (do Admin company quản lý qua mục 4.5) — nhân sự
    /// chỉ xem, không tự sửa được các trường này trên hồ sơ của mình.
    /// </summary>
    public class ProfileModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // ===== Hồ sơ cơ bản (mục 4.2), nhân sự tự chỉnh sửa được =====
        public string FullName { get; set; }
        public string AvatarUrl { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Bio { get; set; }

        public List<SocialLinkModel> SocialLinks { get; set; } = new();
        public List<EducationHistoryModel> EducationHistory { get; set; } = new();
        public List<WorkExperienceModel> WorkHistory { get; set; } = new();

        // ===== Kỹ năng cá nhân (mục 4.3): tự khai báo, endorsement do người khác xác nhận =====
        public List<SkillItemModel> Skills { get; set; } = new();

        // ===== Lịch sử tham gia dự án trên hệ thống (mục 4.2), tự động tổng hợp — chỉ đọc =====
        public List<ProjectHistorySummary> ProjectHistory { get; set; } = new();

        // ===== Đồng bộ một chiều từ StaffModel (Modules.Companies) — chỉ đọc trên hồ sơ (mục 4.5) =====
        public string CompanyName { get; set; }
        public string Position { get; set; }
        public string Department { get; set; }
        public StaffStatus Status { get; set; } = StaffStatus.Active;
        public DateTime JoinDate { get; set; } = DateTime.Now;
        public DateTime? LeftDate { get; set; }

        // ===== Quyền riêng tư (mục 4.2) =====
        public ProfileVisibility Visibility { get; set; } = ProfileVisibility.CompanyOnly;
        public bool ShowContactInfo { get; set; } = true;
        public bool ShowProjectHistory { get; set; } = true;

        // ===== Thuộc tính hiển thị =====
        public string Initial => string.IsNullOrWhiteSpace(FullName) ? "?" : FullName.Trim()[0].ToString().ToUpper();

        public int? Age => DateOfBirth.HasValue
            ? DateTime.Now.Year - DateOfBirth.Value.Year - (DateTime.Now.DayOfYear < DateOfBirth.Value.DayOfYear ? 1 : 0)
            : (int?)null;

        /// <summary>Thâm niên làm việc tại công ty, tính tới LeftDate nếu đã nghỉ, hoặc tới hiện tại.</summary>
        public string TenureDisplay
        {
            get
            {
                var end = LeftDate ?? DateTime.Now;
                var months = (end.Year - JoinDate.Year) * 12 + end.Month - JoinDate.Month;
                if (months < 1) return "Mới gia nhập";

                var years = months / 12;
                var remMonths = months % 12;

                if (years > 0)
                    return remMonths > 0 ? $"{years} năm {remMonths} tháng" : $"{years} năm";

                return $"{remMonths} tháng";
            }
        }

        public int CompletedProjectCount => ProjectHistory?.Count(p => p.IsCompleted) ?? 0;
        public int OngoingProjectCount => ProjectHistory?.Count(p => !p.IsCompleted) ?? 0;
    }
}