using System;

namespace TaskMind.WPFs.Modules.Admins.Models
{
    public enum CompanyStatus
    {
        Pending,    // Chờ duyệt
        Active,     // Đang hoạt động
        Suspended,  // Tạm ngưng
        Rejected    // Từ chối
    }

    public class CompanyModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string TaxCode { get; set; }
        public string Field { get; set; }
        public string Email { get; set; }

        /// <summary>Số điện thoại liên hệ của công ty.</summary>
        public string Phone { get; set; }

        /// <summary>Địa chỉ trụ sở, dùng khi Admin tạo/xem chi tiết công ty.</summary>
        public string Address { get; set; }

        public string Package { get; set; }
        public CompanyStatus Status { get; set; }
        public DateTime JoinedDate { get; set; }
        public int StaffCount { get; set; }
        public int ProjectCount { get; set; }
    }
}