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
    public class DetailSkillVM : ViewModelBase
    {
        private readonly Action _onBack;

        public string SkillId { get; }

        private DetailSkillModel _detail = new DetailSkillModel();
        public DetailSkillModel Detail
        {
            get => _detail;
            set
            {
                _detail = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPending));
            }
        }

        /// <summary>true nếu đây là một đề xuất đang chờ duyệt (chưa vào danh mục chính thức).</summary>
        public bool IsPending => Detail?.Skill != null && !Detail.Skill.IsApproved;

        private Geometry _chartGeometry = Geometry.Empty;
        public Geometry ChartGeometry
        {
            get => _chartGeometry;
            set { _chartGeometry = value; OnPropertyChanged(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand ApproveCommand { get; }
        public ICommand RejectCommand { get; }
        public ICommand DeleteCommand { get; }

        /// <summary>
        /// skillId: mã kỹ năng cần xem chi tiết.
        /// onBack: callback do SkillVM cung cấp để điều hướng quay lại chính SkillVM hiện tại
        /// (giữ nguyên filter/search đang chọn).
        /// </summary>
        public DetailSkillVM(string skillId, Action onBack)
        {
            SkillId = skillId;
            _onBack = onBack;

            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            BackCommand = new RelayCommand(_ => _onBack?.Invoke());
            ApproveCommand = new RelayCommand(_ => Approve());
            RejectCommand = new RelayCommand(_ => Reject());
            DeleteCommand = new RelayCommand(_ => Delete());

            _ = LoadDataAsync();
        }

        private void Approve()
        {
            if (Detail?.Skill == null) return;

            Detail.Skill.IsApproved = true;

            // TODO: gọi service PUT /skills/{id}/approve
            AppendHistory("Duyệt kỹ năng", "Kỹ năng được Admin duyệt vào danh mục chính thức.");

            OnPropertyChanged(nameof(Detail));
            OnPropertyChanged(nameof(IsPending));
        }

        private void Reject()
        {
            // TODO: gọi service DELETE hoặc PUT /skills/{id}/reject, sau đó điều hướng quay lại danh sách
            _onBack?.Invoke();
        }

        private void Delete()
        {
            // TODO: gọi service DELETE /skills/{id}, sau đó điều hướng quay lại danh sách
            _onBack?.Invoke();
        }

        private void AppendHistory(string action, string description)
        {
            Detail.ApprovalHistory.Insert(0, new AuditLogEntryModel
            {
                Id = Guid.NewGuid().ToString("N")[..8],
                EntityId = SkillId,
                Action = action,
                Description = description,
                PerformedBy = "Admin",
                Timestamp = DateTime.Now
            });
        }

        /// <summary>
        /// TODO: thay bằng gọi service/API thực tế lấy chi tiết kỹ năng theo SkillId:
        /// số liệu sử dụng, người dùng tiêu biểu, kỹ năng liên quan, lịch sử duyệt.
        /// </summary>
        private async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            await Task.Delay(400);

            var model = new DetailSkillModel
            {
                Skill = new SkillModel
                {
                    Id = SkillId,
                    Name = "React",
                    Category = SkillCategory.Framework,
                    Level = SkillLevel.Intermediate,
                    IsApproved = true,
                    CreatedDate = new DateTime(2023, 2, 1),
                    UsageCount = 342
                },
                TotalProjectsRequiring = 58,
                TotalEndorsements = 210
            };

            model.UsageBySource.Add(new SkillUsageBySourceItem { SourceLabel = "Cá nhân (User)", Count = 180, Percentage = 52.6 });
            model.UsageBySource.Add(new SkillUsageBySourceItem { SourceLabel = "Nhân sự công ty", Count = 112, Percentage = 32.7 });
            model.UsageBySource.Add(new SkillUsageBySourceItem { SourceLabel = "Học viên/giảng viên", Count = 50, Percentage = 14.7 });

            model.TopUsers.Add(new SkillUserItem { UserId = "U003", UserName = "Phạm Gia Huy", Level = SkillLevel.Advanced, EndorsementCount = 12 });
            model.TopUsers.Add(new SkillUserItem { UserId = "U002", UserName = "Lê Minh Khoa", Level = SkillLevel.Intermediate, EndorsementCount = 8 });
            model.TopUsers.Add(new SkillUserItem { UserId = "U001", UserName = "Trần Thị Bích", Level = SkillLevel.Intermediate, EndorsementCount = 5 });

            model.RelatedSkills.Add(new SkillModel { Id = "K003", Name = "JavaScript", Category = SkillCategory.ProgrammingLanguage, Level = SkillLevel.Intermediate });
            model.RelatedSkills.Add(new SkillModel { Id = "K005", Name = "ASP.NET Core", Category = SkillCategory.Framework, Level = SkillLevel.Advanced });

            model.ApprovalHistory.Add(new AuditLogEntryModel
            {
                Id = "SH1",
                EntityId = SkillId,
                Action = "Thêm vào danh mục",
                Description = "Kỹ năng được Admin tạo trực tiếp.",
                PerformedBy = "Admin",
                Timestamp = new DateTime(2023, 2, 1)
            });

            var labels = new[] { "T1", "T2", "T3", "T4", "T5", "T6", "T7" };
            var rnd = new Random(SkillId?.GetHashCode() ?? 1);
            double baseValue = 200;
            foreach (var label in labels)
            {
                baseValue += rnd.Next(-5, 25);
                model.GrowthChart.Add(new ChartPoint { Label = label, Value = baseValue });
            }

            Detail = model;
            ChartGeometry = BuildChartGeometry(Detail.GrowthChart, 560, 140, 10);

            IsBusy = false;
        }

        /// <summary>Dựng Geometry cho line chart, giống pattern đã dùng ở DashbroadVM/DetailCompanyVM/DetailSchoolVM.</summary>
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