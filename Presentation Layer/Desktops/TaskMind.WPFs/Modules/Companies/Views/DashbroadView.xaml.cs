using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Companies.Models;
using TaskMind.WPFs.Modules.Companies.ViewModels;

namespace TaskMind.WPFs.Modules.Companies.Views
{
    public partial class DashbroadView : UserControl
    {
        public DashbroadView()
        {
            InitializeComponent();
            DataContextChanged += DashbroadView_DataContextChanged;
            Loaded += (_, _) => DrawChart();
        }

        private void DashbroadView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is DashbroadVM oldVm)
                oldVm.PropertyChanged -= Vm_PropertyChanged;

            if (e.NewValue is DashbroadVM newVm)
                newVm.PropertyChanged += Vm_PropertyChanged;

            DrawChart();
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // ProjectProgress là ObservableCollection nên không raise PropertyChanged khi Add/Clear từng item,
            // nhưng ta gọi lại DrawChart mỗi khi IsBusy chuyển từ true -> false (load xong dữ liệu).
            if (e.PropertyName == nameof(DashbroadVM.IsBusy))
                DrawChart();
        }

        private void ChartHost_SizeChanged(object sender, SizeChangedEventArgs e) => DrawChart();

        private void DrawChart()
        {
            if (DataContext is not DashbroadVM vm) return;
            if (ChartHost.ActualWidth <= 0 || ChartHost.ActualHeight <= 0) return;

            var points = vm.ProjectProgress?.ToList();
            if (points == null || points.Count == 0)
            {
                ProgressLine.Points = null;
                return;
            }

            double width = ChartHost.ActualWidth;
            double height = ChartHost.ActualHeight;

            double max = points.Max(p => p.Value);
            double min = points.Min(p => p.Value);
            double range = max - min == 0 ? 1 : max - min;
            double stepX = points.Count == 1 ? 0 : width / (points.Count - 1);

            var result = new PointCollection();
            for (int i = 0; i < points.Count; i++)
            {
                double x = i * stepX;
                // padding trên/dưới 10% để đường không dính sát mép
                double y = height * 0.9 - ((points[i].Value - min) / range * height * 0.8);
                result.Add(new Point(x, y));
            }

            ProgressLine.Points = result;
        }
    }
}