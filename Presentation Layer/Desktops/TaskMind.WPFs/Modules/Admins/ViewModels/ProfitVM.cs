using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using TaskMind.WPFs.Modules.Admins.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Admins.ViewModels
{
    public class ProfitVM : ViewModelBase
    {
        private ProfitSummary _summary = new ProfitSummary();
        public ProfitSummary Summary
        {
            get => _summary;
            set { _summary = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ChartPoint> RevenueChart { get; } = new ObservableCollection<ChartPoint>();

        private Geometry _chartGeometry = Geometry.Empty;
        public Geometry ChartGeometry
        {
            get => _chartGeometry;
            set { _chartGeometry = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ProfitTransactionModel> Transactions { get; } = new ObservableCollection<ProfitTransactionModel>();

        private ICollectionView _transactionsView;
        public ICollectionView TransactionsView
        {
            get => _transactionsView;
            private set { _transactionsView = value; OnPropertyChanged(); }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); TransactionsView?.Refresh(); }
        }

        /// <summary>"All" | tên ProfitSource</summary>
        private string _sourceFilter = "All";
        public string SourceFilter
        {
            get => _sourceFilter;
            set { _sourceFilter = value; OnPropertyChanged(); TransactionsView?.Refresh(); }
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

        public ProfitVM()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            FilterSourceCommand = new RelayCommand(f => SourceFilter = f as string ?? "All");
            IssueInvoiceCommand = new RelayCommand(IssueInvoice);

            TransactionsView = CollectionViewSource.GetDefaultView(Transactions);
            TransactionsView.Filter = FilterTransactions;
            TransactionsView.SortDescriptions.Add(new System.ComponentModel.SortDescription(
                nameof(ProfitTransactionModel.Date), System.ComponentModel.ListSortDirection.Descending));

            _ = LoadDataAsync();
        }

        private bool FilterTransactions(object obj)
        {
            if (obj is not ProfitTransactionModel t) return false;

            if (SourceFilter != "All" &&
                !string.Equals(t.Source.ToString(), SourceFilter, StringComparison.OrdinalIgnoreCase))
                return false;

            if (!string.IsNullOrWhiteSpace(SearchText) &&
                t.PartnerName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) < 0)
                return false;

            return true;
        }

        private void IssueInvoice(object obj)
        {
            if (obj is ProfitTransactionModel t && t.InvoiceStatus == InvoiceStatus.Pending)
            {
                t.InvoiceStatus = InvoiceStatus.Issued;
                // TODO: gọi service POST /invoices để xuất hoá đơn thật (liên kết mục 5.5)
                Touch(t);
            }
        }

        /// <summary>ProfitTransactionModel chưa implement INotifyPropertyChanged nên cần "chạm" lại item.</summary>
        private void Touch(ProfitTransactionModel changed)
        {
            int index = Transactions.IndexOf(changed);
            if (index >= 0)
            {
                Transactions.RemoveAt(index);
                Transactions.Insert(index, changed);
            }
        }

        /// <summary>
        /// TODO: thay bằng gọi service/API thực tế lấy báo cáo doanh thu + danh sách giao dịch.
        /// </summary>
        private async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            await Task.Delay(400);

            Transactions.Clear();
            foreach (var t in new[]
            {
                new ProfitTransactionModel { Id="T001", PartnerName="FPT Software", PartnerType=PartnerType.Company, Source=ProfitSource.MembershipFee, Amount=15_000_000, Date=new DateTime(2026,7,1), InvoiceStatus=InvoiceStatus.Paid },
                new ProfitTransactionModel { Id="T002", PartnerName="DataWise Corp", PartnerType=PartnerType.Company, Source=ProfitSource.TransactionFee, Amount=4_200_000, OriginalTransactionValue=84_000_000, Date=new DateTime(2026,7,3), InvoiceStatus=InvoiceStatus.Issued },
                new ProfitTransactionModel { Id="T003", PartnerName="FUNiX Academy", PartnerType=PartnerType.School, Source=ProfitSource.MembershipFee, Amount=12_000_000, Date=new DateTime(2026,7,5), InvoiceStatus=InvoiceStatus.Paid },
                new ProfitTransactionModel { Id="T004", PartnerName="CloudBase JSC", PartnerType=PartnerType.Company, Source=ProfitSource.TransactionFee, Amount=1_800_000, OriginalTransactionValue=36_000_000, Date=new DateTime(2026,7,8), InvoiceStatus=InvoiceStatus.Pending },
                new ProfitTransactionModel { Id="T005", PartnerName="Học viện Công nghệ ABC", PartnerType=PartnerType.School, Source=ProfitSource.MembershipFee, Amount=18_000_000, Date=new DateTime(2026,7,10), InvoiceStatus=InvoiceStatus.Overdue },
                new ProfitTransactionModel { Id="T006", PartnerName="Vietsoft Solutions", PartnerType=PartnerType.Company, Source=ProfitSource.TransactionFee, Amount=2_500_000, OriginalTransactionValue=50_000_000, Date=new DateTime(2026,7,12), InvoiceStatus=InvoiceStatus.Pending },
                new ProfitTransactionModel { Id="T007", PartnerName="ByteForge", PartnerType=PartnerType.Company, Source=ProfitSource.MembershipFee, Amount=6_000_000, Date=new DateTime(2026,7,13), InvoiceStatus=InvoiceStatus.Issued },
            })
            {
                Transactions.Add(t);
            }

            decimal txFee = Transactions.Where(t => t.Source == ProfitSource.TransactionFee).Sum(t => t.Amount);
            decimal memberFee = Transactions.Where(t => t.Source == ProfitSource.MembershipFee).Sum(t => t.Amount);

            Summary = new ProfitSummary
            {
                TransactionFeeRevenue = txFee,
                MembershipFeeRevenue = memberFee,
                TotalRevenue = txFee + memberFee,
                GrowthPercent = 12.4
            };

            RevenueChart.Clear();
            foreach (var point in new[]
            {
                new ChartPoint { Label = "T1", Value = 42_000_000 },
                new ChartPoint { Label = "T2", Value = 48_000_000 },
                new ChartPoint { Label = "T3", Value = 45_000_000 },
                new ChartPoint { Label = "T4", Value = 52_000_000 },
                new ChartPoint { Label = "T5", Value = 58_000_000 },
                new ChartPoint { Label = "T6", Value = 65_000_000 },
                new ChartPoint { Label = "T7", Value = (double)Summary.TotalRevenue },
            })
            {
                RevenueChart.Add(point);
            }

            ChartGeometry = BuildChartGeometry(RevenueChart, width: 600, height: 160, padding: 12);

            IsBusy = false;
        }

        /// <summary>Dựng Geometry cho line chart, giống pattern đã dùng ở DashbroadVM.</summary>
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