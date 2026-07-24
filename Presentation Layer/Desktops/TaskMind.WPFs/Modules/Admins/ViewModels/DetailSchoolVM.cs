using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Admins.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Admins.ViewModels
{
    public class DetailSchoolVM : ViewModelBase
    {
        private readonly Action _onBack;

        public string SchoolId { get; }

        private DetailSchoolModel _detail = new DetailSchoolModel();
        public DetailSchoolModel Detail
        {
            get => _detail;
            set { _detail = value; OnPropertyChanged(); }
        }

        private Geometry _studentChartGeometry = Geometry.Empty;
        public Geometry StudentChartGeometry
        {
            get => _studentChartGeometry;
            set { _studentChartGeometry = value; OnPropertyChanged(); }
        }

        private Geometry _courseChartGeometry = Geometry.Empty;
        public Geometry CourseChartGeometry
        {
            get => _courseChartGeometry;
            set { _courseChartGeometry = value; OnPropertyChanged(); }
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
        /// schoolId: mã cơ sở đào tạo cần xem chi tiết.
        /// onBack: callback gọi khi bấm "Quay lại", do SchoolVM cung cấp để điều hướng
        /// ngược lại về chính SchoolVM hiện tại (giữ nguyên filter/search đang chọn).
        /// </summary>
        public DetailSchoolVM(string schoolId, Action onBack)
        {
            SchoolId = schoolId;
            _onBack = onBack;

            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            BackCommand = new RelayCommand(_ => _onBack?.Invoke());
            ToggleSuspendCommand = new RelayCommand(_ => ToggleSuspend());

            _ = LoadDataAsync();
        }

        private void ToggleSuspend()
        {
            if (Detail?.School == null) return;

            Detail.School.Status = Detail.School.Status == SchoolStatus.Suspended
                ? SchoolStatus.Active
                : SchoolStatus.Suspended;

            // TODO: gọi service PUT /schools/{id}/suspend hoặc /activate
            OnPropertyChanged(nameof(Detail));
        }

        /// <summary>
        /// TODO: thay bằng gọi service/API thực tế lấy chi tiết cơ sở đào tạo theo SchoolId:
        /// thông tin cơ sở, đánh giá, báo cáo vi phạm (cơ sở + nhân sự), biểu đồ tăng trưởng.
        /// </summary>
        private async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            await Task.Delay(400);

            var model = new DetailSchoolModel
            {
                School = new SchoolModel
                {
                    Id = SchoolId,
                    Name = "FUNiX Academy",
                    Field = "Công nghệ phần mềm",
                    Email = "info@funix.edu.vn",
                    Phone = "024 3782 2233",
                    Address = "Toà nhà FPT, đường Duy Tân, Cầu Giấy, Hà Nội",
                    Package = "Enterprise",
                    Status = SchoolStatus.Active,
                    JoinedDate = new DateTime(2022, 4, 12),
                    TeacherCount = 65,
                    CourseCount = 30,
                    StudentCount = 3400
                },
                AverageRating = 4.5,
                TotalReviews = 128,
                EmploymentRateAfter2Years = 88.2
            };

            model.Reviews.Add(new SchoolReviewModel { Id = "SRV1", ReviewerName = "Nguyễn Thị Hằng", Rating = 5, Comment = "Chương trình học sát với thực tế công việc, giảng viên hỗ trợ nhiệt tình.", CreatedDate = DateTime.Now.AddDays(-8) });
            model.Reviews.Add(new SchoolReviewModel { Id = "SRV2", ReviewerName = "Trần Quốc Bảo", Rating = 4, Comment = "Học phí hợp lý, tuy nhiên số lượng dự án thực hành còn ít.", CreatedDate = DateTime.Now.AddDays(-20) });
            model.Reviews.Add(new SchoolReviewModel { Id = "SRV3", ReviewerName = "Lê Thị Ngọc", Rating = 5, Comment = "Được giới thiệu việc làm ngay sau khi tốt nghiệp.", CreatedDate = DateTime.Now.AddMonths(-1) });

            model.Reports.Add(new ReportModel
            {
                Id = "R020",
                ReporterName = "Phạm Văn Long",
                ReportedEntityId = SchoolId,
                ReportedEntityName = model.School.Name,
                ReportedEntityType = ReportedEntityType.School,
                ViolationType = ViolationType.FakeInformation,
                Priority = ReportPriority.Medium,
                Description = "Quảng cáo sai lệch về tỉ lệ có việc làm sau tốt nghiệp trên fanpage.",
                Status = ReportStatus.Resolved,
                CreatedDate = DateTime.Now.AddMonths(-4),
                Resolution = new ResolutionModel
                {
                    Action = ResolutionAction.Warning,
                    Note = "Đã cảnh cáo, cơ sở cam kết chỉnh sửa nội dung quảng cáo.",
                    ResolvedBy = "Admin",
                    ResolvedDate = DateTime.Now.AddMonths(-4).AddDays(3)
                }
            });

            model.Reports.Add(new ReportModel
            {
                Id = "R021",
                ReporterName = "Vũ Thị Mai",
                ReportedEntityId = "TEACHER-012",
                ReportedEntityName = "Nguyễn Văn Giảng (giảng viên FUNiX Academy)",
                ReportedEntityType = ReportedEntityType.User,
                ViolationType = ViolationType.Harassment,
                Priority = ReportPriority.High,
                Description = "Giảng viên có lời lẽ không phù hợp với học viên trong lớp học trực tuyến.",
                Status = ReportStatus.Pending,
                CreatedDate = DateTime.Now.AddDays(-3)
            });

            var labels = new[] { "T1", "T2", "T3", "T4", "T5", "T6", "T7" };
            var rnd = new Random(SchoolId?.GetHashCode() ?? 1);

            double studentBase = 3000;
            foreach (var label in labels)
            {
                studentBase += rnd.Next(-20, 120);
                model.StudentGrowthChart.Add(new ChartPoint { Label = label, Value = studentBase });
            }

            double courseBase = 22;
            foreach (var label in labels)
            {
                courseBase += rnd.Next(-1, 4);
                model.CourseGrowthChart.Add(new ChartPoint { Label = label, Value = courseBase });
            }

            Detail = model;
            StudentChartGeometry = BuildChartGeometry(Detail.StudentGrowthChart, 560, 140, 10);
            CourseChartGeometry = BuildChartGeometry(Detail.CourseGrowthChart, 560, 140, 10);

            IsBusy = false;
        }

        /// <summary>Dựng Geometry cho line chart, giống pattern đã dùng ở DashbroadVM/ReportVM/ProfitVM/DetailCompanyVM.</summary>
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