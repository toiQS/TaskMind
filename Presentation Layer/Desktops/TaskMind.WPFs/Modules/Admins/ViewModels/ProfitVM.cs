using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using MediatR;
using TaskMind.Applications.Admins.Features.Profit;
using TaskMind.WPFs.Modules.Admins.Mapping;
using TaskMind.WPFs.Modules.Admins.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Admins.ViewModels
{
    public class ProfitVM : ViewModelBase
    {
        private readonly IMediator _mediator;

        private ProfitSummary _summary = new ProfitSummary();
        public ProfitSummary Summary
        {
            get => _summary;
            set { _summary = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// GetProfitSummaryQuery không trả chuỗi theo tháng — chỉ có tổng hợp hiện tại.
        /// RevenueChart giữ dạng 1 điểm duy nhất (TotalRevenue) cho tới khi có Query theo thời gian.
        /// TODO: bổ sung GetMonthlyRevenueQuery ở Application.Admins nếu cần chart nhiều điểm thật.
        /// </summary>
        public ObservableCollection<ChartPoint> RevenueChart { get; } = new();

        private Geometry _chartGeometry = Geometry.Empty;
        public Geometry ChartGeometry
        {
            get => _chartGeometry;
            set { _chartGeometry = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ProfitTransactionModel> Transactions { get; } = new();

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); _ = LoadTransactionsAsync(); }
        }

        /// <summary>"All" | tên ProfitSource</summary>
        private string _sourceFilter = "All";
        public string SourceFilter
        {
            get => _sourceFilter;
            set { _sourceFilter = value; OnPropertyChanged(); _ = LoadTransactionsAsync(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand FilterSourceCommand { get; }
        public ICommand IssueInvoiceCommand { get; }

        public ProfitVM() : this(null) { }

        public ProfitVM(IMediator mediator)
        {
            _mediator = MediatorResolver.Resolve(mediator);

            RefreshCommand = new RelayCommand(async _ => await LoadAllAsync());
            FilterSourceCommand = new RelayCommand(f => SourceFilter = f as string ?? "All");
            IssueInvoiceCommand = new RelayCommand(async o => await IssueInvoiceAsync(o));

            _ = LoadAllAsync();
        }

        private async Task LoadAllAsync()
        {
            await LoadSummaryAsync();
            await LoadTransactionsAsync();
        }

        private async Task LoadSummaryAsync()
        {
            if (_mediator == null) return;

            var dto = await _mediator.Send(new GetProfitSummaryQuery { RecentInvoiceCount = 10 });
            Summary = ProfitUiMapper.ToUi(dto);

            RevenueChart.Clear();
            RevenueChart.Add(new ChartPoint { Label = "Hiện tại", Value = (double)Summary.TotalRevenue });
            ChartGeometry = BuildChartGeometry(RevenueChart, 600, 160, 12);
        }

        private async Task LoadTransactionsAsync()
        {
            if (_mediator == null || IsBusy) return;
            IsBusy = true;

            var dtos = await _mediator.Send(new GetInvoicesQuery
            {
                SearchText = SearchText,
                SourceFilter = SourceFilter
            });

            Transactions.Clear();
            foreach (var dto in dtos)
                Transactions.Add(ProfitUiMapper.ToUi(dto));

            IsBusy = false;
        }

        private async Task IssueInvoiceAsync(object obj)
        {
            if (obj is not ProfitTransactionModel t || t.InvoiceStatus != InvoiceStatus.Pending) return;

            var dto = await _mediator.Send(new IssueInvoiceCommand { InvoiceId = Guid.Parse(t.Id) });
            t.InvoiceStatus = Enum.Parse<InvoiceStatus>(dto.Status);
            Touch(t);
        }

        private void Touch(ProfitTransactionModel changed)
        {
            int index = Transactions.IndexOf(changed);
            if (index >= 0)
            {
                Transactions.RemoveAt(index);
                Transactions.Insert(index, changed);
            }
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