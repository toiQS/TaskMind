using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Companies.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Companies.ViewModels
{
    public class StoreVM : ViewModelBase
    {
        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); ApplyFilter(); } }

        private ListingType? _typeFilter;
        public ListingType? TypeFilter { get => _typeFilter; set { _typeFilter = value; OnPropertyChanged(); ApplyFilter(); } }

        private ListingStatus? _statusFilter;
        public ListingStatus? StatusFilter { get => _statusFilter; set { _statusFilter = value; OnPropertyChanged(); ApplyFilter(); } }

        private StoreListingModel _selectedListing;
        public StoreListingModel SelectedListing
        {
            get => _selectedListing;
            set { _selectedListing = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasSelectedListing)); }
        }
        public bool HasSelectedListing => SelectedListing != null;

        public ObservableCollection<StoreListingModel> Listings { get; } = new();
        public ObservableCollection<StoreListingModel> FilteredListings { get; } = new();

        public int PendingCount => Listings.Count(l => l.Status == ListingStatus.PendingApproval);
        public int PublishedCount => Listings.Count(l => l.Status == ListingStatus.Published);
        public int SoldCount => Listings.Count(l => l.Status == ListingStatus.Sold);

        public ICommand RefreshCommand { get; }
        public ICommand CreateListingCommand { get; }
        public ICommand OpenDetailCommand { get; }
        public ICommand CloseDetailCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand SetTypeFilterCommand { get; }
        public ICommand SetStatusFilterCommand { get; }
        public ICommand ApproveCommand { get; }
        public ICommand RejectCommand { get; }
        public ICommand CloseListingCommand { get; }

        public StoreVM()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadAsync());
            CreateListingCommand = new RelayCommand(_ => CreateListing());
            OpenDetailCommand = new RelayCommand(p => SelectedListing = p as StoreListingModel);
            CloseDetailCommand = new RelayCommand(_ => SelectedListing = null);
            ClearFilterCommand = new RelayCommand(_ => { SearchText = string.Empty; TypeFilter = null; StatusFilter = null; });
            SetTypeFilterCommand = new RelayCommand(p => TypeFilter = p is ListingType t ? t : (ListingType?)null);
            SetStatusFilterCommand = new RelayCommand(p => StatusFilter = p is ListingStatus s ? s : (ListingStatus?)null);
            ApproveCommand = new RelayCommand(p => UpdateStatus(p as StoreListingModel, ListingStatus.Published));
            RejectCommand = new RelayCommand(p => UpdateStatus(p as StoreListingModel, ListingStatus.Rejected));
            CloseListingCommand = new RelayCommand(p => UpdateStatus(p as StoreListingModel, ListingStatus.Closed));

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            IsBusy = true;

            // TODO: gọi service GET /store/listings thay cho dữ liệu mẫu bên dưới
            await Task.Delay(400);

            Listings.Clear();

            Listings.Add(new StoreListingModel
            {
                Title = "Hệ thống quản lý kho (đã ngừng phát triển)",
                Description = "Dự án ASP.NET Core + Angular quản lý xuất nhập kho, dùng nội bộ 2 năm, nay công ty đổi hướng nên muốn trao đổi/bán lại toàn bộ mã nguồn.",
                Type = ListingType.Project,
                Status = ListingStatus.PendingApproval,
                TechStack = new() { "ASP.NET Core", "Angular", "SQL Server" },
                Price = 45_000_000m,
                IsNegotiable = true,
                MilestoneBasedPayment = true,
                SellerName = "Ngô Quốc Huy",
                SellerCompany = "Kho Vận Miền Nam",
                RepoUrl = "https://github.com/example/warehouse-system",
                CreatedDate = DateTime.Now.AddDays(-1)
            });

            Listings.Add(new StoreListingModel
            {
                Title = "Thư viện xử lý ảnh open source cho .NET",
                Description = "Thư viện resize/crop/nén ảnh hiệu năng cao viết bằng C#, đang cần thêm contributor để phát triển tiếp, hoàn toàn miễn phí.",
                Type = ListingType.OpenSource,
                Status = ListingStatus.Published,
                TechStack = new() { "C#", ".NET 10", "Image Processing" },
                Price = null,
                SellerName = "Lê Thị Hoa",
                RepoUrl = "https://github.com/example/image-toolkit",
                DemoUrl = "https://example-imagetoolkit.dev",
                CreatedDate = DateTime.Now.AddDays(-6),
                ViewCount = 128,
                InterestCount = 14
            });

            Listings.Add(new StoreListingModel
            {
                Title = "App đặt lịch khám bệnh (MVP)",
                Description = "Sản phẩm MVP hoàn chỉnh gồm mobile app + backend, đã có 500 người dùng thử nghiệm, muốn chuyển nhượng do đổi hướng kinh doanh.",
                Type = ListingType.Project,
                Status = ListingStatus.Negotiating,
                TechStack = new() { "Flutter", "NestJS", "PostgreSQL" },
                Price = 120_000_000m,
                IsNegotiable = true,
                SellerName = "Trần Văn Bình",
                SellerCompany = "HealthTech Startup",
                CreatedDate = DateTime.Now.AddDays(-14),
                ViewCount = 340,
                InterestCount = 22
            });

            Listings.Add(new StoreListingModel
            {
                Title = "CLI tool tạo boilerplate dự án React",
                Description = "Công cụ dòng lệnh giúp khởi tạo nhanh cấu trúc dự án React chuẩn, đã dùng nội bộ nhiều dự án đào tạo, chia sẻ miễn phí cho cộng đồng.",
                Type = ListingType.OpenSource,
                Status = ListingStatus.Published,
                TechStack = new() { "Node.js", "React", "CLI" },
                Price = null,
                SellerName = "Đỗ Thu Trang",
                RepoUrl = "https://github.com/example/react-boilerplate-cli",
                CreatedDate = DateTime.Now.AddMonths(-1),
                ViewCount = 560,
                InterestCount = 47
            });

            Listings.Add(new StoreListingModel
            {
                Title = "Landing page template lỗi thời (không rõ nguồn gốc)",
                Description = "Tin đăng thiếu thông tin bản quyền mã nguồn, không xác định được người đóng góp gốc.",
                Type = ListingType.Project,
                Status = ListingStatus.Rejected,
                TechStack = new() { "HTML", "CSS" },
                Price = 2_000_000m,
                SellerName = "Phạm Minh Tuấn",
                CreatedDate = DateTime.Now.AddDays(-20),
                AdminNote = "Từ chối do không chứng minh được quyền sở hữu mã nguồn."
            });

            ApplyFilter();
            RaiseCounters();
            IsBusy = false;
        }

        private void ApplyFilter()
        {
            var query = Listings.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(l =>
                    l.Title?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                    l.TechStackDisplay.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            if (TypeFilter.HasValue) query = query.Where(l => l.Type == TypeFilter.Value);
            if (StatusFilter.HasValue) query = query.Where(l => l.Status == StatusFilter.Value);

            FilteredListings.Clear();
            foreach (var l in query.OrderByDescending(l => l.CreatedDate))
                FilteredListings.Add(l);
        }

        private void UpdateStatus(StoreListingModel listing, ListingStatus status)
        {
            if (listing == null) return;

            // TODO: gọi service PATCH /store/listings/{id}/status
            listing.Status = status;
            Touch();
        }

        private void CreateListing()
        {
            // TODO: mở dialog/điều hướng "Đăng tin bán/trao đổi", gọi service POST /store/listings
        }

        private void Touch()
        {
            ApplyFilter();
            RaiseCounters();
            if (SelectedListing != null)
            {
                var updated = SelectedListing;
                SelectedListing = null;
                SelectedListing = updated;
            }
        }

        private void RaiseCounters()
        {
            OnPropertyChanged(nameof(PendingCount));
            OnPropertyChanged(nameof(PublishedCount));
            OnPropertyChanged(nameof(SoldCount));
        }
    }
}