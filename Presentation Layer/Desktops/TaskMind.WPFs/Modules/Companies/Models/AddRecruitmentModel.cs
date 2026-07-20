using System;
using System.Collections.Generic;

namespace TaskMind.WPFs.Modules.Companies.Models
{
    /// <summary>
    /// Dữ liệu form "Đăng tin tuyển dụng mới" (mục 5.1 - Quản lý tuyển dụng và ứng tuyển).
    /// Dùng để tập hợp input từ AddRecruitmentVM trước khi build thành JobPostingModel thật khi lưu.
    /// </summary>
    public class AddRecruitmentModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }

        public JobLevel Level { get; set; } = JobLevel.Junior;
        public EmploymentType EmploymentType { get; set; } = EmploymentType.FullTime;

        public List<string> RequiredSkills { get; set; } = new();

        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public bool SalaryNegotiable { get; set; }

        public DateTime? Deadline { get; set; }
    }
}