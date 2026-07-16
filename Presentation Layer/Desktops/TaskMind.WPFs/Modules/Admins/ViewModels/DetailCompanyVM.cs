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
    public class DetailCompanyVM : ViewModelBase
    {
        private readonly Action _onBack;

        public string CompanyId { get; }

        private DetailCompanyModel _detail = new DetailCompanyModel();
        public DetailCompanyModel Detail
        {
            get => _detail;
            set { _detail = value; OnPropertyChanged(); }
        }

        private Geometry _staffChartGeometry = Geometry.Empty;
        public Geometry StaffChartGeometry
        {
            get => _staffChartGeometry;
            set { _staffChartGeometry = value; OnPropertyChanged(); }
        }

        private Geometry _projectChartGeometry = Geometry.Empty;
        public Geometry ProjectChartGeometry
        {
            get => _projectChartGeometry;
            set { _projectChartGeometry = value; OnPropertyChanged(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand ToggleSuspendCommand { get; }

        /// <summary>
        /// companyId: mã công ty cần xem chi tiết.
        /// onBack: callback gọi khi bấm "Quay lại", do CompanyVM cung cấp để điều hướng
        /// ngược lại về chính CompanyVM hiện tại (giữ nguyên filter/search đang chọn).
        /// </summary>
        public DetailCompanyVM(string companyId, Action onBack)
        {
            CompanyId = companyId;
            _onBack = onBack;

            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            BackCommand = new RelayCommand(_ => _onBack?.Invoke());
            ToggleSuspendCommand = new RelayCommand(_ => ToggleSuspend());

            _ = LoadDataAsync();
        }

        private void ToggleSuspend()
        {
            if (Detail?.Company == null) return;

            Detail.Company.Status = Detail.Company.Status == CompanyStatus.Suspended
                ? CompanyStatus.Active
                : CompanyStatus.Suspended;

            // TODO: gọi service PUT /companies/{id}/suspend hoặc /activate
            OnPropertyChanged(nameof(Detail));
        }

        /// <summary>
        /// TODO: thay bằng gọi service/API thực tế lấy chi tiết công ty theo CompanyId:
        /// thông tin công ty, đánh giá, báo cáo vi phạm (công ty + nhân sự), biểu đồ tăng trưởng.
        /// </summary>
        private async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            await Task.Delay(400);

            var model = new DetailCompanyModel
            {
                Company = new CompanyModel
                {
                    Id = CompanyId,
                    Name = "FPT Software",
                    TaxCode = "0102030405",
                    Field = "Phát triển phần mềm",
                    Email = "contact@fpt.com",
                    Phone = "024 7300 8866",
                    Address = "Toà nhà FPT, đường Duy Tân, Cầu Giấy, Hà Nội",
                    Package = "Enterprise",
                    Status = CompanyStatus.Active,
                    JoinedDate = new DateTime(2024, 3, 10),
                    StaffCount = 128,
                    ProjectCount = 34
                },
                AverageRating = 4.3,
                TotalReviews = 56,
                EmploymentRateAfter2Years = 82.5
            };

            model.Reviews.Add(new CompanyReviewModel { Id = "RV1", ReviewerName = "Trần Thị Bích", Rating = 5, Comment = "Môi trường làm việc chuyên nghiệp, dự án thực tế rất bổ ích.", CreatedDate = DateTime.Now.AddDays(-10) });
            model.Reviews.Add(new CompanyReviewModel { Id = "RV2", ReviewerName = "Lê Minh Khoa", Rating = 4, Comment = "Quản lý dự án rõ ràng nhưng deadline hơi gấp.", CreatedDate = DateTime.Now.AddDays(-25) });
            model.Reviews.Add(new CompanyReviewModel { Id = "RV3", ReviewerName = "Phạm Gia Huy", Rating = 4, Comment = "Được Technical leader hướng dẫn tận tình.", CreatedDate = DateTime.Now.AddMonths(-2) });

            model.Reports.Add(new ReportModel
            {
                Id = "R010",
                ReporterName = "Nguyễn Văn A",
                ReportedEntityId = CompanyId,
                ReportedEntityName = model.Company.Name,
                ReportedEntityType = ReportedEntityType.Company,
                ViolationType = ViolationType.FraudPayment,
                Priority = ReportPriority.Medium,
                Description = "Chậm thanh toán milestone cho freelancer trong dự án trao đổi.",
                Status = ReportStatus.Resolved,
                CreatedDate = DateTime.Now.AddMonths(-3),
                Resolution = new ResolutionModel
                {
                    Action = ResolutionAction.Warning,
                    Note = "Đã cảnh cáo, công ty cam kết thanh toán đúng hạn.",
                    ResolvedBy = "Admin",
                    ResolvedDate = DateTime.Now.AddMonths(-3).AddDays(2)
                }
            });

            model.Reports.Add(new ReportModel
            {
                Id = "R011",
                ReporterName = "Đặng Hải Yến",
                ReportedEntityId = "STAFF-045",
                ReportedEntityName = "Nguyễn Văn Staff (nhân sự FPT Software)",
                ReportedEntityType = ReportedEntityType.User,
                ViolationType = ViolationType.Harassment,
                Priority = ReportPriority.High,
                Description = "Nhân sự có hành vi quấy rối thành viên nhóm dự án chung.",
                Status = ReportStatus.Pending,
                CreatedDate = DateTime.Now.AddDays(-2)
            });

            var labels = new[] { "T1", "T2", "T3", "T4", "T5", "T6", "T7" };
            var rnd = new Random(CompanyId?.GetHashCode() ?? 1);

            double staffBase = 90;
            foreach (var label in labels)
            {
                staffBase += rnd.Next(-3, 10);
                model.StaffGrowthChart.Add(new ChartPoint { Label = label, Value = staffBase });
            }

            double projectBase = 20;
            foreach (var label in labels)
            {
                projectBase += rnd.Next(-2, 6);
                model.ProjectGrowthChart.Add(new ChartPoint { Label = label, Value = projectBase });
            }

            Detail = model;
            StaffChartGeometry = BuildChartGeometry(Detail.StaffGrowthChart, 560, 140, 10);
            ProjectChartGeometry = BuildChartGeometry(Detail.ProjectGrowthChart, 560, 140, 10);

            IsBusy = false;
        }

        /// <summary>Dựng Geometry cho line chart, giống pattern đã dùng ở DashbroadVM/ReportVM/ProfitVM.</summary>
        private Geometry BuildChartGeometry(ObservableCollection<ChartPoint> points, double width, double height, double padding)
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