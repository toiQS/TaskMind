using System;
using System.Collections.ObjectModel;

namespace TaskMind.WPFs.Modules.Admins.Models
{
    /// <summary>Một đánh giá dành cho công ty (mục 5.2 - Quản lý đánh giá và phản hồi).</summary>
    public class CompanyReviewModel
    {
        public string Id { get; set; }
        public string ReviewerName { get; set; }

        /// <summary>Thang điểm 1-5.</summary>
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// Dữ liệu chi tiết của một công ty, dùng cho DetailCompanyView.
    /// Gộp: thông tin công ty, báo cáo vi phạm (công ty + nhân sự trực thuộc),
    /// đánh giá công ty, biểu đồ tăng trưởng theo thời gian thực.
    /// </summary>
    public class DetailCompanyModel
    {
        public CompanyModel Company { get; set; } = new CompanyModel();

        // ----- Đánh giá công ty -----
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }

        /// <summary>% nhân sự/sinh viên có việc làm sau 2 năm kể từ khi tham gia (ưu tiên hiển thị theo mục 5.2).</summary>
        public double EmploymentRateAfter2Years { get; set; }
        public ObservableCollection<CompanyReviewModel> Reviews { get; set; } = new ObservableCollection<CompanyReviewModel>();

        // ----- Báo cáo vi phạm liên quan: cả công ty lẫn nhân sự (Staff) trực thuộc -----
        public ObservableCollection<ReportModel> Reports { get; set; } = new ObservableCollection<ReportModel>();

        // ----- Live chart: tăng trưởng nhân sự / dự án theo tháng -----
        public ObservableCollection<ChartPoint> StaffGrowthChart { get; set; } = new ObservableCollection<ChartPoint>();
        public ObservableCollection<ChartPoint> ProjectGrowthChart { get; set; } = new ObservableCollection<ChartPoint>();
    }
}