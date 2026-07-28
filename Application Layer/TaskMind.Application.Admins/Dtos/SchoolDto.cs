namespace TaskMind.Applications.Admins.Dtos
{
    public class SchoolDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Package { get; set; } = string.Empty;

        /// <summary>"Pending" | "Active" | "Suspended" | "Rejected" (xem VerifiableEntityStatusHelper)</summary>
        public string Status { get; set; } = string.Empty;
        public DateTime JoinedDateUtc { get; set; }
    }

    public class SchoolDetailDto : SchoolDto
    {
        public string Address { get; set; } = string.Empty;
        public int TeacherCount { get; set; }
        public int StudentCount { get; set; }
        public int ProjectCount { get; set; }

        /// <summary>
        /// Chưa có Domain entity "Course" trong Domain Layer hiện tại nên luôn trả về 0.
        /// Bổ sung khi mục 4.8 (Quản lý khoá học) được mô hình hoá ở tầng Domain.
        /// </summary>
        public int CourseCount { get; set; }
    }
}