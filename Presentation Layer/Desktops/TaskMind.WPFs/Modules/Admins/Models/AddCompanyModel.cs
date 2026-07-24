namespace TaskMind.WPFs.Modules.Admins.Models
{
    /// <summary>
    /// Dữ liệu form "Thêm công ty mới" do Admin nhập trực tiếp.
    /// Khác với CompanyModel (đã có Id/Status/thống kê), model này chỉ chứa
    /// những trường mà Admin cần khai báo tại thời điểm tạo.
    /// </summary>
    public class AddCompanyModel
    {
        public string Name { get; set; }
        public string TaxCode { get; set; }
        public string Field { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

        /// <summary>"Starter" | "Pro" | "Enterprise"</summary>
        public string Package { get; set; } = "Starter";
    }
}