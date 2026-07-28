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
    }

    public class CompanyDetailDto : CompanyDto
    {
        public string Address { get; set; } = string.Empty;
        public int StaffCount { get; set; }
        public int ProjectCount { get; set; }
    }
}