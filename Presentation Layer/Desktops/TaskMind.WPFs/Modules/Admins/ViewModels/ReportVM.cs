using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Admins.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Admins.ViewModels
{
    public class ReportVM : ViewModelBase
    {
        private ReportSummary _summary = new ReportSummary();
        public ReportSummary Summary
        {
            get => _summary;
            set { _summary = value; OnPropertyChanged(); }
        }

        public ObservableCollection<RevenuePoint> RevenueTrend { get; } = new ObservableCollection<RevenuePoint>();

        public ObservableCollection<RevenueBySourceItem> RevenueBySource { get; } = new ObservableCollection<RevenueBySourceItem>();

        public ObservableCollection<TransactionRecord> RecentTransactions { get; } = new ObservableCollection<TransactionRecord>();

        private Geometry _chartGeometry = Geometry.Empty;
        public Geometry ChartGeometry
        {
            get => _chartGeometry;
            set { _chartGeometry = value; OnPropertyChanged(); }
        }

        /// <summary>"Week" | "Month" | "Quarter" | "Year"</summary>
        private string _periodFilter = "Month";
        public string PeriodFilter
        {
            get => _periodFilter;
            set
            {
                if (_periodFilter == value) return;
                _periodFilter = value;
                OnPropertyChanged();
                _ = LoadDataAsync();
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand FilterPeriodCommand { get; }
        public ICommand ExportCommand { get; }

        public ReportVM()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            FilterPeriodCommand = new RelayCommand(p => PeriodFilter = p as string ?? "Month");
            ExportCommand = new RelayCommand(async _ => await ExportAsync());

            _ = LoadDataAsync();
        }

        /// <summary>
        /// TODO: thay bằng gọi service/API thực tế lấy báo cáo doanh thu theo kỳ (PeriodFilter).
        /// Hiện tại đang seed dữ liệu mẫu để dựng giao diện.
        /// </summary>
        private async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            await Task.Delay(400);

            string[] labels = PeriodFilter switch
            {
                "Week" => new[] { "T2", "T3", "T4", "T5", "T6", "T7", "CN" },
                "Quarter" => new[] { "Q1", "Q2", "Q3", "Q4" },
                "Year" => new[] { "2022", "2023", "2024", "2025", "2026" },
                _ => new[] { "T1", "T2", "T3", "T4", "T5", "T6", "T7" }
            };

            var rnd = new Random(labels.Length);
            RevenueTrend.Clear();
            double baseValue = 120;
            foreach (var label in labels)
            {
                baseValue += rnd.Next(-20, 60);
                if (baseValue < 40) baseValue = 40;
                RevenueTrend.Add(new RevenuePoint { Label = label, Value = Math.Round(baseValue, 0) });
            }
            ChartGeometry = BuildChartGeometry(RevenueTrend, width: 600, height: 160, padding: 12);

            decimal transactionFee = 182_500_000m;
            decimal companySub = 96_000_000m;
            decimal schoolSub = 54_500_000m;
            decimal total = transactionFee + companySub + schoolSub;

            Summary = new ReportSummary
            {
                TotalRevenue = total,
                RevenueDeltaPercent = 8.4,
                TransactionFeeRevenue = transactionFee,
                SubscriptionRevenue = companySub + schoolSub,
                TotalTransactions = 214
            };

            RevenueBySource.Clear();
            foreach (var item in new[]
            {
                new RevenueBySourceItem { Source = RevenueSourceType.TransactionFee, Amount = transactionFee, Percentage = (double)(transactionFee / total * 100) },
                new RevenueBySourceItem { Source = RevenueSourceType.CompanySubscription, Amount = companySub, Percentage = (double)(companySub / total * 100) },
                new RevenueBySourceItem { Source = RevenueSourceType.SchoolSubscription, Amount = schoolSub, Percentage = (double)(schoolSub / total * 100) },
            })
            {
                RevenueBySource.Add(item);
            }

            RecentTransactions.Clear();
            foreach (var t in new[]
            {
                new TransactionRecord { Id = "GD2607", PartnerName = "FPT Software ↔ DataWise Corp", Source = RevenueSourceType.TransactionFee, Amount = 4_200_000m, Date = new DateTime(2026,7,13) },
                new TransactionRecord { Id = "GD2606", PartnerName = "CloudBase JSC", Source = RevenueSourceType.CompanySubscription, Amount = 2_000_000m, Date = new DateTime(2026,7,12) },
                new TransactionRecord { Id = "GD2605", PartnerName = "FUNiX Academy", Source = RevenueSourceType.SchoolSubscription, Amount = 3_500_000m, Date = new DateTime(2026,7,11) },
                new TransactionRecord { Id = "GD2604", PartnerName = "NextGen Tech ↔ ByteForge", Source = RevenueSourceType.TransactionFee, Amount = 1_800_000m, Date = new DateTime(2026,7,10) },
                new TransactionRecord { Id = "GD2603", PartnerName = "Vietsoft Solutions", Source = RevenueSourceType.CompanySubscription, Amount = 2_000_000m, Date = new DateTime(2026,7,8) },
            })
            {
                RecentTransactions.Add(t);
            }

            IsBusy = false;
        }

        /// <summary>TODO: gọi service xuất báo cáo (PDF/Excel) cho kỳ đang chọn.</summary>
        private async Task ExportAsync()
        {
            await Task.CompletedTask;
            // TODO: gọi service GET /reports/export?period={PeriodFilter}
        }

        /// <summary>Dựng Geometry cho line chart, giống cách làm ở DashbroadVM, không cần thư viện chart ngoài.</summary>
        private Geometry BuildChartGeometry(ObservableCollection<RevenuePoint> points, double width, double height, double padding)
        {
            if (points == null || points.Count == 0)
                return Geometry.Empty;

            double max = points.Max(p => p.Value);
            double min = points.Min(p => p.Value);
            if (max == min) max = min + 1;

            double stepX = points.Count > 1 ? (width - padding * 2) / (points.Count - 1) : 0;

            var geometry = new StreamGeometry();
            using (StreamGeometryContext ctx = geometry.Open())
            {
                for (int i = 0; i < points.Count; i++)
                {
                    double x = padding + i * stepX;
                    double normalized = (points[i].Value - min) / (max - min);
                    double y = height - padding - normalized * (height - padding * 2);

                    if (i == 0)
                        ctx.BeginFigure(new Point(x, y), isFilled: false, isClosed: false);
                    else
                        ctx.LineTo(new Point(x, y), isStroked: true, isSmoothJoin: true);
                }
            }
            geometry.Freeze();
            return geometry;
        }
    }
}