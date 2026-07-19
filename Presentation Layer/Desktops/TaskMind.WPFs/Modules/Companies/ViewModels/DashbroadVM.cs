using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Companies.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Companies.ViewModels
{
    public class DashbroadVM : ViewModelBase
    {
        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        public ObservableCollection<QuickStatModel> QuickStats { get; } = new();
        public ObservableCollection<ProjectProgressSummary> TopProjects { get; } = new();
        public ObservableCollection<RecentActivityModel> RecentActivities { get; } = new();
        public ObservableCollection<TopCandidateSummary> TopCandidates { get; } = new();

        public ICommand RefreshCommand { get; }

        public DashbroadVM()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadAsync());
            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            IsBusy = true;

            // TODO: gọi service GET /company/{companyId}/dashboard/summary thay cho dữ liệu mẫu bên dưới,
            // tổng hợp từ ProjectVM/StaffVM/RecruitmentVM/SupportVM/CandidateVM.
            await Task.Delay(400);

            QuickStats.Clear();
            QuickStats.Add(new QuickStatModel
            {
                Title = "Dự án đang thực hiện",
                Value = "2",
                SubText = "trên tổng số 4 dự án",
                Icon = "Board24",
                AccentColor = "#4C9AFF",
                Trend = TrendDirection.Up,
                TrendPercent = 8
            });
            QuickStats.Add(new QuickStatModel
            {
                Title = "Nhân sự đang hoạt động",
                Value = "4",
                SubText = "1 đang tạm ngưng",
                Icon = "People24",
                AccentColor = "#3FD07A",
                Trend = TrendDirection.Stable
            });
            QuickStats.Add(new QuickStatModel
            {
                Title = "Tin tuyển dụng đang mở",
                Value = "2",
                SubText = "4 ứng viên mới chưa xem",
                Icon = "PersonAdd24",
                AccentColor = "#9A7BFF",
                Trend = TrendDirection.Up,
                TrendPercent = 15
            });
            QuickStats.Add(new QuickStatModel
            {
                Title = "Yêu cầu hỗ trợ chờ xử lý",
                Value = "1",
                SubText = "trên tổng số 5 yêu cầu",
                Icon = "ChatMultiple24",
                AccentColor = "#FFC14C",
                Trend = TrendDirection.Down,
                TrendPercent = 20
            });

            TopProjects.Clear();
            TopProjects.Add(new ProjectProgressSummary { Name = "Hệ thống ERP nội bộ", Progress = 62, Status = ProjectStatus.InProgress, TaskDone = 30, TaskTotal = 48 });
            TopProjects.Add(new ProjectProgressSummary { Name = "Website thương mại điện tử ABC", Progress = 35, Status = ProjectStatus.InProgress, TaskDone = 21, TaskTotal = 60 });
            TopProjects.Add(new ProjectProgressSummary { Name = "Nền tảng học trực tuyến", Progress = 45, Status = ProjectStatus.Paused, TaskDone = 18, TaskTotal = 40 });

            RecentActivities.Clear();
            RecentActivities.Add(new RecentActivityModel
            {
                Title = "Ứng viên mới ứng tuyển",
                Description = "Nguyễn Văn A vừa nộp hồ sơ cho vị trí Backend Developer (.NET).",
                Time = DateTime.Now.AddHours(-1),
                IconSymbol = "PersonAdd24",
                AccentColor = "#4C9AFF"
            });
            RecentActivities.Add(new RecentActivityModel
            {
                Title = "Yêu cầu hỗ trợ mới",
                Description = "Đề xuất nâng cấp RAM cho dàn máy dev đang chờ duyệt.",
                Time = DateTime.Now.AddHours(-5),
                IconSymbol = "ChatMultiple24",
                AccentColor = "#FFC14C"
            });
            RecentActivities.Add(new RecentActivityModel
            {
                Title = "Nhân sự mới gia nhập",
                Description = "Phạm Thị D chính thức gia nhập vị trí QA/QC Intern.",
                Time = DateTime.Now.AddDays(-1),
                IconSymbol = "People24",
                AccentColor = "#3FD07A"
            });
            RecentActivities.Add(new RecentActivityModel
            {
                Title = "Cập nhật tiến độ dự án",
                Description = "Website thương mại điện tử ABC hoàn thành milestone 2.",
                Time = DateTime.Now.AddDays(-2),
                IconSymbol = "Board24",
                AccentColor = "#9A7BFF"
            });

            TopCandidates.Clear();
            TopCandidates.Add(new TopCandidateSummary { FullName = "Phạm Thị D", JobTitle = "Thực tập sinh QA/QC", MatchScore = 91 });
            TopCandidates.Add(new TopCandidateSummary { FullName = "Nguyễn Văn A", JobTitle = "Backend Developer (.NET)", MatchScore = 88 });
            TopCandidates.Add(new TopCandidateSummary { FullName = "Trần Thị B", JobTitle = "Backend Developer (.NET)", MatchScore = 72 });

            IsBusy = false;
        }
    }
}