using System;

namespace TaskMind.WPFs.Modules.Admins.Models
{
    public enum UserAccountStatus
    {
        Active,   // Đang hoạt động
        Locked,   // Tạm khoá
        Banned    // Bị cấm
    }

    public enum UserType
    {
        Student,          // Sinh viên
        JobSeeker,        // Người tìm việc
        OssContributor    // Người đóng góp mã nguồn mở
    }

    public class UserModel
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; }
        public UserType Type { get; set; }
        public UserAccountStatus Status { get; set; }
        public DateTime JoinedDate { get; set; }
        public DateTime LastActiveDate { get; set; }
        public int SkillCount { get; set; }
        public int ProjectCount { get; set; }
    }
}