using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Admins.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Admins.ViewModels
{
    public class DashbroadVM : ViewModelBase
    {
        private DashbroadStatistic _statistic = new DashbroadStatistic();
        public DashbroadStatistic Statistic
        {
            get => _statistic;
            set { _statistic = value; OnPropertyChanged(); }
        }

        public ObservableCollection<TodoModel> TodoList { get; } = new ObservableCollection<TodoModel>();

        public ObservableCollection<ChartPoint> RevenueChart { get; } = new ObservableCollection<ChartPoint>();

        private Geometry _chartGeometry = Geometry.Empty;
        /// <summary>
        /// Đường line chart đã được dựng sẵn (Path.Data bind trực tiếp vào đây)
        /// </summary>
        public Geometry ChartGeometry
        {
            get => _chartGeometry;
            set { _chartGeometry = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }

        public DashbroadVM()
        {
            RefreshCommand = new RelayCommand(_ => LoadData());
            LoadData();
        }

        /// <summary>
        /// TODO: thay bằng gọi service/API thực tế lấy dữ liệu Dashboard.
        /// Hiện tại đang seed dữ liệu mẫu để dựng giao diện.
        /// </summary>
        private void LoadData()
        {
            Statistic = new DashbroadStatistic
            {
                CountAllUsers = 1240,
                CountNewUsers = 38,
                CountAllProject = 312,
                CountNewProjects = 12,
                CountAllCompanies = 54,
                CountNewCompanies = 3,
                CountAllSchools = 21,
                CountNewSchools = 1,
                CountAllTeachers = 87,
                CountNewTeacher = 4,
                CountAllStaff = 210,
                CountNewStaff = 9
            };

            TodoList.Clear();
            foreach (var todo in new[]
            {
                new TodoModel { Index = "1", Name = "Duyệt công ty mới đăng ký", PriorityLevel = 1 },
                new TodoModel { Index = "2", Name = "Xử lý báo cáo vi phạm", PriorityLevel = 1 },
                new TodoModel { Index = "3", Name = "Duyệt đề xuất kỹ năng mới", PriorityLevel = 2 },
                new TodoModel { Index = "4", Name = "Kiểm tra hoá đơn tháng", PriorityLevel = 3 },
            })
            {
                TodoList.Add(todo);
            }

            RevenueChart.Clear();
            foreach (var point in new[]
            {
                new ChartPoint { Label = "T1", Value = 120 },
                new ChartPoint { Label = "T2", Value = 180 },
                new ChartPoint { Label = "T3", Value = 150 },
                new ChartPoint { Label = "T4", Value = 220 },
                new ChartPoint { Label = "T5", Value = 260 },
                new ChartPoint { Label = "T6", Value = 300 },
            })
            {
                RevenueChart.Add(point);
            }

            ChartGeometry = BuildChartGeometry(RevenueChart, width: 600, height: 160, padding: 12);
        }

        /// <summary>
        /// Dựng Geometry cho line chart từ danh sách ChartPoint, không cần thư viện chart ngoài.
        /// </summary>
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