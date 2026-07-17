using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Companies.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Companies.ViewModels
{
    public class DashbroadVM : ViewModelBase
    {
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ObservableCollection<StatCardModel> StatCards { get; } = new();
        public ObservableCollection<ChartPointModel> ProjectProgress { get; } = new();
        public ObservableCollection<ActivityItemModel> RecentActivities { get; } = new();

        public ICommand RefreshCommand { get; }

        public DashbroadVM()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadDashboardAsync());
            _ = LoadDashboardAsync();
        }

        private async Task LoadDashboardAsync()
        {
            IsBusy = true;

            // TODO: gọi service GET /company/dashboard thay cho dữ liệu mẫu bên dưới
            await Task.Delay(400);

            StatCards.Clear();
            StatCards.Add(new StatCardModel { Title = "Nhân sự hoạt động", Value = "24", Icon = "People24", TrendText = "+3 tháng này", IsTrendPositive = true });
            StatCards.Add(new StatCardModel { Title = "Dự án đang chạy", Value = "7", Icon = "Folder24", TrendText = "+1 tuần này", IsTrendPositive = true });
            StatCards.Add(new StatCardModel { Title = "Bài kiểm tra chờ duyệt", Value = "5", Icon = "DocumentCheckmark24", TrendText = "-2 so với tuần trước", IsTrendPositive = false });
            StatCards.Add(new StatCardModel { Title = "Tỷ lệ hoàn thành", Value = "82%", Icon = "ChartMultiple24", TrendText = "+5% so với quý trước", IsTrendPositive = true });

            ProjectProgress.Clear();
            var months = new[] { "T1", "T2", "T3", "T4", "T5", "T6" };
            var values = new double[] { 40, 55, 48, 63, 70, 82 };
            for (int i = 0; i < months.Length; i++)
                ProjectProgress.Add(new ChartPointModel { Label = months[i], Value = values[i] });

            RecentActivities.Clear();
            RecentActivities.Add(new ActivityItemModel { Title = "Dự án ERP nội bộ", Description = "Đến hạn milestone Sprint 3", Time = DateTime.Now.AddHours(-1), Priority = ActivityPriority.High });
            RecentActivities.Add(new ActivityItemModel { Title = "Bài test tuyển dụng Backend", Description = "3 ứng viên chờ chấm", Time = DateTime.Now.AddHours(-4), Priority = ActivityPriority.Medium });
            RecentActivities.Add(new ActivityItemModel { Title = "Nhân viên mới", Description = "Nguyễn Văn A vừa được thêm vào Staff", Time = DateTime.Now.AddDays(-1), Priority = ActivityPriority.Low });

            IsBusy = false;
        }
    }
}