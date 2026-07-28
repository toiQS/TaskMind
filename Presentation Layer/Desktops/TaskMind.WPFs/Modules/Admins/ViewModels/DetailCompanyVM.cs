using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using MediatR;
using TaskMind.Applications.Admins.Features.Companies;
using TaskMind.WPFs.Modules.Admins.Mapping;
using TaskMind.WPFs.Modules.Admins.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Admins.ViewModels
{
    public class DetailCompanyVM : ViewModelBase
    {
        private readonly Action _onBack;
        private readonly IMediator _mediator;

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

        public DetailCompanyVM(string companyId, Action onBack, IMediator mediator)
        {
            CompanyId = companyId;
            _onBack = onBack;
            _mediator = MediatorResolver.Resolve(mediator);

            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            BackCommand = new RelayCommand(_ => _onBack?.Invoke());
            ToggleSuspendCommand = new RelayCommand(async _ => await ToggleSuspendAsync());

            _ = LoadDataAsync();
        }

        private async Task ToggleSuspendAsync()
        {
            if (Detail?.Company == null) return;

            var dto = await _mediator.Send(new ToggleSuspendCompanyCommand { CompanyId = Guid.Parse(CompanyId) });
            Detail.Company.Status = Enum.Parse<CompanyStatus>(dto.Status);
            OnPropertyChanged(nameof(Detail));
        }

        /// <summary>
        /// Thông tin cơ bản (Company + StaffCount/ProjectCount/Address) đến từ GetCompanyDetailQuery thật.
        /// Reviews, Reports (mục 5.2/5.7), StaffGrowthChart/ProjectGrowthChart theo tháng CHƯA có Query
        /// tương ứng ở Application.Admins — giữ nguyên seed mẫu cho các phần này (đánh dấu TODO)
        /// cho tới khi Domain có Review/AuditTrail-theo-tháng aggregate.
        /// </summary>
        private async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            var dto = await _mediator.Send(new GetCompanyDetailQuery { CompanyId = Guid.Parse(CompanyId) });

            var model = new DetailCompanyModel { Company = CompanyUiMapper.ToUi(dto) };
            CompanyUiMapper.ApplyDetail(model.Company, dto);

            // TODO: thay bằng GetCompanyReviewsQuery / GetCompanyReportsQuery khi có ở Application.Admins.
            model.AverageRating = 0;
            model.TotalReviews = 0;
            model.EmploymentRateAfter2Years = 0;

            // TODO: thay bằng GetCompanyGrowthChartQuery (StaffCount/ProjectCount theo tháng) khi có.
            var labels = new[] { "T1", "T2", "T3", "T4", "T5", "T6", "T7" };
            foreach (var label in labels)
            {
                model.StaffGrowthChart.Add(new ChartPoint { Label = label, Value = dto.StaffCount });
                model.ProjectGrowthChart.Add(new ChartPoint { Label = label, Value = dto.ProjectCount });
            }

            Detail = model;
            StaffChartGeometry = BuildChartGeometry(Detail.StaffGrowthChart, 560, 140, 10);
            ProjectChartGeometry = BuildChartGeometry(Detail.ProjectGrowthChart, 560, 140, 10);

            IsBusy = false;
        }

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
                        ctx.BeginFigure(new System.Windows.Point(x, y), isFilled: false, isClosed: false);
                    else
                        ctx.LineTo(new System.Windows.Point(x, y), isStroked: true, isSmoothJoin: true);
                }
            }
            geometry.Freeze();
            return geometry;
        }
    }
}