namespace TaskMind.Applications.Admins.Dtos
{
    public class CompanyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Package { get; set; } = string.Empty;

        /// <summary>"Pending" | "Active" | "Suspended" | "Rejected" (xem VerifiableEntityStatusHelper)</summary>
        public string Status { get; set; } = string.Empty;
        public DateTime JoinedDateUtc { get; set; }

        /// <summary>Số nhân sự (Staff) trực thuộc — dùng cho cột thống kê nhanh ở CompanyView.</summary>
        public int StaffCount { get; set; }

        /// <summary>Số dự án trực thuộc công ty (Project.SourceType = Company).</summary>
        public int ProjectCount { get; set; }
    }

    public class CompanyDetailDto : CompanyDto
    {
        public string Address { get; set; } = string.Empty;
    }
}
