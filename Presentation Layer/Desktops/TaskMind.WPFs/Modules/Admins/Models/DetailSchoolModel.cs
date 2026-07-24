using System.Collections.ObjectModel;

namespace TaskMind.WPFs.Modules.Admins.Models
{
    /// <summary>Một đánh giá dành cho cơ sở đào tạo (mục 5.2 - Quản lý đánh giá và phản hồi).</summary>
    public class SchoolReviewModel
    {
        public string Id { get; set; }
        public string ReviewerName { get; set; }

        /// <summary>Thang điểm 1-5.</summary>
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// Dữ liệu chi tiết của một cơ sở đào tạo, dùng cho DetailSchoolView.
    /// Gộp: thông tin cơ sở, báo cáo vi phạm (cơ sở + nhân sự trực thuộc),
    /// đánh giá cơ sở (ưu tiên tỉ lệ có việc làm sau 2 năm tính từ thời điểm tốt nghiệp),
    /// biểu đồ tăng trưởng theo thời gian thực.
    /// </summary>
    public class DetailSchoolModel
    {
        public SchoolModel School { get; set; } = new SchoolModel();

        // ----- Đánh giá cơ sở đào tạo -----
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }

        /// <summary>% học viên có việc làm trong 2 năm đầu tính từ thời điểm tốt nghiệp trên hệ thống (chỉ số ưu tiên theo mục 5.2).</summary>
        public double EmploymentRateAfter2Years { get; set; }
        public ObservableCollection<SchoolReviewModel> Reviews { get; set; } = new ObservableCollection<SchoolReviewModel>();

        // ----- Báo cáo vi phạm liên quan: cả cơ sở lẫn nhân sự (Teacher) trực thuộc -----
        public ObservableCollection<ReportModel> Reports { get; set; } = new ObservableCollection<ReportModel>();

        // ----- Live chart: tăng trưởng học viên / khoá học theo tháng -----
        public ObservableCollection<ChartPoint> StudentGrowthChart { get; set; } = new ObservableCollection<ChartPoint>();
        public ObservableCollection<ChartPoint> CourseGrowthChart { get; set; } = new ObservableCollection<ChartPoint>();
    }
}