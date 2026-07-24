namespace TaskMind.WPFs.Modules.Admins.Models
{
    public enum SchoolStatus
    {
        Pending,    // Chờ duyệt
        Active,     // Đang hoạt động
        Suspended,  // Tạm ngưng
        Rejected    // Từ chối
    }

    public class SchoolModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Field { get; set; }       // Lĩnh vực đào tạo
        public string Email { get; set; }

        /// <summary>Số điện thoại liên hệ của cơ sở đào tạo.</summary>
        public string Phone { get; set; }

        /// <summary>Địa chỉ cơ sở, dùng khi Admin tạo/xem chi tiết.</summary>
        public string Address { get; set; }

        public string Package { get; set; }
        public SchoolStatus Status { get; set; }
        public DateTime JoinedDate { get; set; }
        public int TeacherCount { get; set; }
        public int CourseCount { get; set; }
        public int StudentCount { get; set; }
    }
}