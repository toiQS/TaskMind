using System.Collections.ObjectModel;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Companies.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Companies.ViewModels
{
    public class StoreVM : ViewModelBase
    {
        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        private StoreScope _currentScope = StoreScope.System;
        /// <summary>Thẻ đang xem: Toàn hệ thống hay Dự án của công ty.</summary>
        public StoreScope CurrentScope
        {
            get => _currentScope;
            set { _currentScope = value; OnPropertyChanged(); ApplyFilter(); RaiseCounters(); }
        }

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

        /// <summary>Toàn bộ tin đăng tải từ service (cả của công ty mình lẫn công ty/cá nhân khác).</summary>
        public ObservableCollection<StoreListingModel> Listings { get; } = new();

        /// <summary>Danh sách sau khi áp dụng phạm vi thẻ (System/Company) + tìm kiếm/lọc.</summary>
        public ObservableCollection<StoreListingModel> FilteredListings { get; } = new();

        /// <summary>
        /// Tập tin đăng theo phạm vi thẻ hiện tại:
        /// - System: tin đã duyệt (Published/Negotiating/Sold) của mọi công ty, cộng tin của chính mình dù đang chờ duyệt.
        /// - Company: chỉ tin của chính công ty mình, mọi trạng thái, để tự quản lý.
        /// </summary>
        private IEnumerable<StoreListingModel> ScopedListings =>
            CurrentScope == StoreScope.Company
                ? Listings.Where(l => l.IsMine)
                : Listings.Where(l => l.IsMine || IsPublicStatus(l.Status));

        private static bool IsPublicStatus(ListingStatus s)
            => s is ListingStatus.Published or ListingStatus.Negotiating or ListingStatus.Sold;

        public int PendingCount => ScopedListings.Count(l => l.Status == ListingStatus.PendingApproval);
        public int PublishedCount => ScopedListings.Count(l => l.Status == ListingStatus.Published);
        public int SoldCount => ScopedListings.Count(l => l.Status == ListingStatus.Sold);

        /// <summary>Tổng số tin hiển thị ở mỗi thẻ — dùng cho badge trên nút chuyển thẻ.</summary>
        public int SystemCount => Listings.Count(l => l.IsMine || IsPublicStatus(l.Status));
        public int CompanyCount => Listings.Count(l => l.IsMine);

        public ICommand RefreshCommand { get; }
        public ICommand CreateListingCommand { get; }
        public ICommand OpenDetailCommand { get; }
        public ICommand CloseDetailCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand SetScopeCommand { get; }
        public ICommand SetTypeFilterCommand { get; }
        public ICommand SetStatusFilterCommand { get; }
        public ICommand CloseListingCommand { get; }
        public ICommand MarkSoldCommand { get; }
        public ICommand ExpressInterestCommand { get; }

        public StoreVM()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadAsync());
            CreateListingCommand = new RelayCommand(_ => CreateListing());
            OpenDetailCommand = new RelayCommand(p => SelectedListing = p as StoreListingModel);
            CloseDetailCommand = new RelayCommand(_ => SelectedListing = null);
            ClearFilterCommand = new RelayCommand(_ => { SearchText = string.Empty; TypeFilter = null; StatusFilter = null; });
            SetScopeCommand = new RelayCommand(p => CurrentScope = p is StoreScope sc ? sc : StoreScope.System);
            SetTypeFilterCommand = new RelayCommand(p => TypeFilter = p is ListingType t ? t : (ListingType?)null);
            SetStatusFilterCommand = new RelayCommand(p => StatusFilter = p is ListingStatus s ? s : (ListingStatus?)null);
            CloseListingCommand = new RelayCommand(p => UpdateStatus(p as StoreListingModel, ListingStatus.Closed));
            MarkSoldCommand = new RelayCommand(p => UpdateStatus(p as StoreListingModel, ListingStatus.Sold));
            ExpressInterestCommand = new RelayCommand(p => ExpressInterest(p as StoreListingModel));

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            IsBusy = true;

            // TODO: gọi service GET /store/listings (toàn hệ thống) — backend nên trả kèm field IsMine
            // dựa trên companyId hiện tại, thay cho dữ liệu mẫu bên dưới.
            await Task.Delay(400);

            Listings.Clear();

            // ===== Tin đăng của công ty/cá nhân khác trên toàn hệ thống =====
            Listings.Add(new StoreListingModel
            {
                Title = "Hệ thống quản lý kho (đã ngừng phát triển)",
                Description = "Dự án ASP.NET Core + Angular quản lý xuất nhập kho, dùng nội bộ 2 năm, nay công ty đổi hướng nên muốn trao đổi/bán lại toàn bộ mã nguồn.",
                Type = ListingType.Project,
                Status = ListingStatus.Published,
                TechStack = new() { "ASP.NET Core", "Angular", "SQL Server" },
                Price = 45_000_000m,
                IsNegotiable = true,
                MilestoneBasedPayment = true,
                SellerName = "Ngô Quốc Huy",
                SellerCompany = "Kho Vận Miền Nam",
                RepoUrl = "https://github.com/example/warehouse-system",
                CreatedDate = DateTime.Now.AddDays(-1),
                IsMine = false
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
                InterestCount = 14,
                IsMine = false
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
                InterestCount = 22,
                IsMine = false
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
                InterestCount = 47,
                IsMine = false
            });

            // ===== Tin đăng của chính công ty mình (TaskMind Software JSC) =====
            Listings.Add(new StoreListingModel
            {
                Title = "Module chấm công (ERP bản cũ)",
                Description = "Module chấm công tách rời từ hệ thống ERP nội bộ phiên bản 1, không còn dùng sau khi nâng cấp, có thể tái sử dụng cho công ty quy mô nhỏ.",
                Type = ListingType.Project,
                Status = ListingStatus.Published,
                TechStack = new() { "ASP.NET Core", "SQL Server" },
                Price = 18_000_000m,
                IsNegotiable = true,
                SellerName = "Trần Văn Bình",
                SellerCompany = "TaskMind Software JSC",
                RepoUrl = "https://github.com/taskmind/erp-timesheet-legacy",
                CreatedDate = DateTime.Now.AddDays(-3),
                ViewCount = 42,
                InterestCount = 3,
                IsMine = true
            });

            Listings.Add(new StoreListingModel
            {
                Title = "Nền tảng CRM nội bộ (đang tìm đối tác)",
                Description = "CRM quản lý khách hàng dùng nội bộ, đang trao đổi với 1 đối tác quan tâm để chuyển nhượng toàn bộ.",
                Type = ListingType.Project,
                Status = ListingStatus.Negotiating,
                TechStack = new() { "C#", "Blazor", "PostgreSQL" },
                Price = 60_000_000m,
                IsNegotiable = true,
                SellerName = "Lê Thị Hoa",
                SellerCompany = "TaskMind Software JSC",
                CreatedDate = DateTime.Now.AddDays(-9),
                ViewCount = 76,
                InterestCount = 5,
                IsMine = true
            });

            Listings.Add(new StoreListingModel
            {
                Title = "Bộ SDK tích hợp thanh toán nội bộ",
                Description = "SDK gọi cổng thanh toán VNPay/Momo dùng chung cho các dự án nội bộ, muốn chia sẻ có thu phí nhỏ để bảo trì.",
                Type = ListingType.Project,
                Status = ListingStatus.PendingApproval,
                TechStack = new() { "C#", ".NET 10" },
                Price = 8_000_000m,
                SellerName = "Đỗ Thu Trang",
                SellerCompany = "TaskMind Software JSC",
                CreatedDate = DateTime.Now.AddHours(-20),
                IsMine = true
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
                SellerCompany = "TaskMind Software JSC",
                CreatedDate = DateTime.Now.AddDays(-20),
                AdminNote = "Từ chối do không chứng minh được quyền sở hữu mã nguồn.",
                IsMine = true
            });

            ApplyFilter();
            RaiseCounters();
            IsBusy = false;
        }

        private void ApplyFilter()
        {
            var query = ScopedListings;

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

        private void ExpressInterest(StoreListingModel listing)
        {
            if (listing == null || listing.IsMine) return;

            // TODO: gọi service POST /store/listings/{id}/interest, bắn Notification cho người đăng (mục 5.3)
            listing.InterestCount++;
            Touch();
        }

        private void CreateListing()
        {
            // TODO: mở dialog/điều hướng "Đăng tin bán/trao đổi", gọi service POST /store/listings.
            // Tin mới cần gán IsMine = true, Status = ListingStatus.PendingApproval,
            // có thể tự chuyển CurrentScope = StoreScope.Company để công ty thấy ngay tin vừa đăng.
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
            OnPropertyChanged(nameof(SystemCount));
            OnPropertyChanged(nameof(CompanyCount));
        }
    }
}