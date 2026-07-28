using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using MediatR;
using TaskMind.Applications.Admins.Features.Schools;
using TaskMind.WPFs.Modules.Admins.Mapping;
using TaskMind.WPFs.Modules.Admins.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Admins.ViewModels
{
    public class DetailSchoolVM : ViewModelBase
    {
        private readonly Action _onBack;
        private readonly IMediator _mediator;

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

        public DetailSchoolVM(string schoolId, Action onBack, IMediator mediator)
        {
            SchoolId = schoolId;
            _onBack = onBack;
            _mediator = MediatorResolver.Resolve(mediator);

            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            BackCommand = new RelayCommand(_ => _onBack?.Invoke());
            ToggleSuspendCommand = new RelayCommand(async _ => await ToggleSuspendAsync());

            _ = LoadDataAsync();
        }

        private async Task ToggleSuspendAsync()
        {
            if (Detail?.School == null) return;

            var dto = await _mediator.Send(new ToggleSuspendSchoolCommand { SchoolId = Guid.Parse(SchoolId) });
            Detail.School.Status = Enum.Parse<SchoolStatus>(dto.Status);
            OnPropertyChanged(nameof(Detail));
        }

        /// <summary>
        /// Reviews/Reports/GrowthChart theo tháng: TODO — chưa có Query tương ứng ở Application.Admins.
        /// </summary>
        private async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            var dto = await _mediator.Send(new GetSchoolDetailQuery { SchoolId = Guid.Parse(SchoolId) });

            var model = new DetailSchoolModel { School = SchoolUiMapper.ToUi(dto) };
            SchoolUiMapper.ApplyDetail(model.School, dto);

            model.AverageRating = 0;
            model.TotalReviews = 0;
            model.EmploymentRateAfter2Years = 0;

            var labels = new[] { "T1", "T2", "T3", "T4", "T5", "T6", "T7" };
            foreach (var label in labels)
            {
                model.StudentGrowthChart.Add(new ChartPoint { Label = label, Value = dto.StudentCount });
                model.CourseGrowthChart.Add(new ChartPoint { Label = label, Value = dto.CourseCount });
            }

            Detail = model;
            StudentChartGeometry = BuildChartGeometry(Detail.StudentGrowthChart, 560, 140, 10);
            CourseChartGeometry = BuildChartGeometry(Detail.CourseGrowthChart, 560, 140, 10);

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