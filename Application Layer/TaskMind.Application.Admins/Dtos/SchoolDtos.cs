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

        /// <summary>Số giảng viên (Teacher) trực thuộc — dùng cho cột thống kê nhanh ở SchoolView.</summary>
        public int TeacherCount { get; set; }

        /// <summary>Số học viên (Student) trực thuộc.</summary>
        public int StudentCount { get; set; }

        /// <summary>Số dự án đào tạo trực thuộc (Project.SourceType = School).</summary>
        public int ProjectCount { get; set; }

        /// <summary>
        /// Chưa có Domain entity "Course" trong Domain Layer hiện tại nên luôn trả về 0.
        /// Bổ sung khi mục 4.8 (Quản lý khoá học) được mô hình hoá ở tầng Domain.
        /// </summary>
        public int CourseCount { get; set; }
    }

    public class SchoolDetailDto : SchoolDto
    {
        public string Address { get; set; } = string.Empty;
    }
}
