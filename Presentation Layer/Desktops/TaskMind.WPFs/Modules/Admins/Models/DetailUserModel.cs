using System;
using System.Collections.ObjectModel;

namespace TaskMind.WPFs.Modules.Admins.Models
{
    /// <summary>Một kỹ năng trong hồ sơ cá nhân của user (mục 4.3 - Quản lý kỹ năng cá nhân).</summary>
    public class UserSkillItem
    {
        public string SkillName { get; set; }
        public SkillCategory Category { get; set; }
        public SkillLevel Level { get; set; }

        /// <summary>Số lượt xác nhận (endorsement) từ đồng đội/giảng viên.</summary>
        public int EndorsementCount { get; set; }
    }

    /// <summary>Một dòng lịch sử tham gia dự án của user.</summary>
    public class UserProjectHistoryItem
    {
        public string ProjectName { get; set; }

        /// <summary>Vai trò dự án: Owner, Technical leader, Project manager, QA QC, Developer, Intern.</summary>
        public string ProjectRole { get; set; }

        /// <summary>"Company" | "School" | "OpenSource"</summary>
        public string ProjectSource { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        /// <summary>true = dự án đang thực hiện, false = đã hoàn thành/rời khỏi.</summary>
        public bool IsOngoing { get; set; }
    }

    /// <summary>
    /// Dữ liệu chi tiết của một user, dùng cho DetailUserView.
    /// Gộp: thông tin cá nhân, hồ sơ kỹ năng, lịch sử tham gia dự án,
    /// báo cáo vi phạm liên quan, nhật ký hoạt động tài khoản (mục 5.7 - Audit Log).
    /// </summary>
    public class DetailUserModel
    {
        public UserModel User { get; set; } = new UserModel();

        // ----- Hồ sơ kỹ năng cá nhân -----
        public ObservableCollection<UserSkillItem> Skills { get; set; } = new ObservableCollection<UserSkillItem>();

        // ----- Lịch sử tham gia dự án -----
        public ObservableCollection<UserProjectHistoryItem> ProjectHistory { get; set; } = new ObservableCollection<UserProjectHistoryItem>();

        // ----- Báo cáo vi phạm liên quan đến user này -----
        public ObservableCollection<ReportModel> Reports { get; set; } = new ObservableCollection<ReportModel>();

        // ----- Nhật ký hoạt động tài khoản (mục 5.7 - Audit Log) -----
        public ObservableCollection<AuditLogEntryModel> AuditLogs { get; set; } = new ObservableCollection<AuditLogEntryModel>();
    }
}