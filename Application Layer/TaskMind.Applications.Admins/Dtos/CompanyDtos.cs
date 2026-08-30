using TaskMind.Domain.Commons.ObjectValues;
using TaskMind.Domain.Enums;

namespace TaskMind.Applications.Admins.Dtos
{
    public class CompanyListItemDto
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public EntityStatus Status { get; set; }
        public string MembershipPackage { get; set; } = string.Empty;
        public DateTime JoinDate { get; set; }
    }

    public class CompanyDetailDto
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public Address Address { get; set; } = new();
        public bool IsVerified { get; set; }
        public EntityStatus Status { get; set; }
        public string MembershipPackage { get; set; } = string.Empty;
        public DateTime JoinDate { get; set; }
        public int ActiveStaffCount { get; set; }
        public int TotalProjectCount { get; set; }
    }

    /// <summary>Bộ lọc danh sách công ty cho Admin (mục 4.4: kiểm duyệt/quản lý).</summary>
    public class GetCompaniesFilter
    {
        public bool? IsVerified { get; set; }
        public EntityStatus? Status { get; set; }
        public string? Keyword { get; set; } // tìm theo CompanyName/TaxCode/Email
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}