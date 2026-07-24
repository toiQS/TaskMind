using System.Collections.ObjectModel;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Companies.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Companies.ViewModels
{
    public class FindVM : ViewModelBase
    {
        /// <summary>Công nghệ chủ lực hiện có của công ty mình — dùng để tính mức khớp với nhu cầu của công ty tiềm năng.
        /// TODO: thay bằng danh sách kỹ năng thực tế tổng hợp từ StaffVM khi có service.</summary>
        private static readonly List<string> OurCoreSkills = new()
        {
            "C#", "ASP.NET Core", "SQL Server", "React", "Node.js", "Flutter"
        };

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        private FindScope _currentScope = FindScope.Candidate;
        /// <summary>Thẻ đang xem: Ứng viên tự do hay Khách hàng tiềm năng (công ty).</summary>
        public FindScope CurrentScope
        {
            get => _currentScope;
            set { _currentScope = value; OnPropertyChanged(); }
        }

        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); ApplyFilter(); ApplyCompanyFilter(); } }

        // ===== Bộ lọc riêng cho thẻ Ứng viên =====
        private FreelancerAvailability? _availabilityFilter;
        public FreelancerAvailability? AvailabilityFilter
        {
            get => _availabilityFilter;
            set { _availabilityFilter = value; OnPropertyChanged(); ApplyFilter(); }
        }

        private ProjectNeedModel _selectedProjectNeed;
        public ProjectNeedModel SelectedProjectNeed
        {
            get => _selectedProjectNeed;
            set { _selectedProjectNeed = value; OnPropertyChanged(); }
        }

        private FreelanceCandidateModel _selectedCandidate;
        public FreelanceCandidateModel SelectedCandidate
        {
            get => _selectedCandidate;
            set { _selectedCandidate = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasSelectedCandidate)); }
        }
        public bool HasSelectedCandidate => SelectedCandidate != null;

        // ===== Bộ lọc riêng cho thẻ Khách hàng tiềm năng =====
        private CompanyLeadStatus? _companyStatusFilter;
        public CompanyLeadStatus? CompanyStatusFilter
        {
            get => _companyStatusFilter;
            set { _companyStatusFilter = value; OnPropertyChanged(); ApplyCompanyFilter(); }
        }

        private PotentialCompanyModel _selectedCompany;
        public PotentialCompanyModel SelectedCompany
        {
            get => _selectedCompany;
            set { _selectedCompany = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasSelectedCompany)); }
        }
        public bool HasSelectedCompany => SelectedCompany != null;

        public ObservableCollection<SkillTrendModel> SkillTrends { get; } = new();
        public ObservableCollection<ProjectNeedModel> ProjectNeeds { get; } = new();
        public ObservableCollection<FreelanceCandidateModel> Candidates { get; } = new();
        public ObservableCollection<FreelanceCandidateModel> FilteredCandidates { get; } = new();

        public ObservableCollection<PotentialCompanyModel> Companies { get; } = new();
        public ObservableCollection<PotentialCompanyModel> FilteredCompanies { get; } = new();

        public int NewLeadCount => Companies.Count(c => c.Status == CompanyLeadStatus.New);
        public int InTalksCount => Companies.Count(c => c.Status == CompanyLeadStatus.InTalks);
        public int ConvertedCount => Companies.Count(c => c.Status == CompanyLeadStatus.Converted);

        public ICommand RefreshCommand { get; }
        public ICommand SetScopeCommand { get; }
        public ICommand ClearFilterCommand { get; }

        // Ứng viên
        public ICommand SelectProjectCommand { get; }
        public ICommand OpenCandidateDetailCommand { get; }
        public ICommand CloseCandidateDetailCommand { get; }
        public ICommand SetAvailabilityFilterCommand { get; }
        public ICommand InviteCommand { get; }

        // Khách hàng tiềm năng
        public ICommand OpenCompanyDetailCommand { get; }
        public ICommand CloseCompanyDetailCommand { get; }
        public ICommand SetCompanyStatusFilterCommand { get; }
        public ICommand MarkContactedCommand { get; }
        public ICommand MarkInTalksCommand { get; }
        public ICommand MarkConvertedCommand { get; }
        public ICommand MarkNotInterestedCommand { get; }
        public ICommand SendExchangeRequestCommand { get; }

        public FindVM()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadAsync());
            SetScopeCommand = new RelayCommand(p => CurrentScope = p is FindScope sc ? sc : FindScope.Candidate);
            ClearFilterCommand = new RelayCommand(_ =>
            {
                SearchText = string.Empty;
                AvailabilityFilter = null;
                CompanyStatusFilter = null;
            });

            SelectProjectCommand = new RelayCommand(p => SelectProject(p as ProjectNeedModel));
            OpenCandidateDetailCommand = new RelayCommand(p => SelectedCandidate = p as FreelanceCandidateModel);
            CloseCandidateDetailCommand = new RelayCommand(_ => SelectedCandidate = null);
            SetAvailabilityFilterCommand = new RelayCommand(p => AvailabilityFilter = p is FreelancerAvailability a ? a : (FreelancerAvailability?)null);
            InviteCommand = new RelayCommand(p => Invite(p as FreelanceCandidateModel));

            OpenCompanyDetailCommand = new RelayCommand(p => SelectedCompany = p as PotentialCompanyModel);
            CloseCompanyDetailCommand = new RelayCommand(_ => SelectedCompany = null);
            SetCompanyStatusFilterCommand = new RelayCommand(p => CompanyStatusFilter = p is CompanyLeadStatus s ? s : (CompanyLeadStatus?)null);
            MarkContactedCommand = new RelayCommand(p => UpdateCompanyStatus(p as PotentialCompanyModel, CompanyLeadStatus.Contacted));
            MarkInTalksCommand = new RelayCommand(p => UpdateCompanyStatus(p as PotentialCompanyModel, CompanyLeadStatus.InTalks));
            MarkConvertedCommand = new RelayCommand(p => UpdateCompanyStatus(p as PotentialCompanyModel, CompanyLeadStatus.Converted));
            MarkNotInterestedCommand = new RelayCommand(p => UpdateCompanyStatus(p as PotentialCompanyModel, CompanyLeadStatus.NotInterested));
            SendExchangeRequestCommand = new RelayCommand(p => SendExchangeRequest(p as PotentialCompanyModel));

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            IsBusy = true;

            // TODO: gọi service GET /company/{companyId}/find/trends, /find/project-needs,
            // /find/candidates, /find/potential-companies thay cho dữ liệu mẫu bên dưới.
            await Task.Delay(400);

            SkillTrends.Clear();
            SkillTrends.Add(new SkillTrendModel { SkillName = "React", DemandCount = 128, ChangePercent = 18.5, Direction = TrendDirection.Up });
            SkillTrends.Add(new SkillTrendModel { SkillName = "ASP.NET Core", DemandCount = 96, ChangePercent = 9.2, Direction = TrendDirection.Up });
            SkillTrends.Add(new SkillTrendModel { SkillName = "Flutter", DemandCount = 74, ChangePercent = 4.1, Direction = TrendDirection.Stable });
            SkillTrends.Add(new SkillTrendModel { SkillName = "PHP", DemandCount = 40, ChangePercent = -12.3, Direction = TrendDirection.Down });
            SkillTrends.Add(new SkillTrendModel { SkillName = "DevOps/CI-CD", DemandCount = 63, ChangePercent = 21.7, Direction = TrendDirection.Up });

            ProjectNeeds.Clear();
            ProjectNeeds.Add(new ProjectNeedModel
            {
                ProjectName = "Hệ thống ERP nội bộ",
                RequiredSkills = new() { "C#", "ASP.NET Core", "SQL Server" },
                OpenSlots = 2
            });
            ProjectNeeds.Add(new ProjectNeedModel
            {
                ProjectName = "Website thương mại điện tử ABC",
                RequiredSkills = new() { "React", "Node.js", "MongoDB" },
                OpenSlots = 3
            });
            ProjectNeeds.Add(new ProjectNeedModel
            {
                ProjectName = "App quản lý kho (bảo trì)",
                RequiredSkills = new() { "Flutter", "Firebase" },
                OpenSlots = 1
            });

            Candidates.Clear();
            Candidates.Add(new FreelanceCandidateModel
            {
                FullName = "Hoàng Minh Khôi",
                Headline = "Fullstack Developer, chuyên React + Node.js",
                Skills = new() { "React", "Node.js", "MongoDB", "TypeScript" },
                ExperienceYears = 4,
                Rating = 4.8,
                CompletedProjects = 12,
                Availability = FreelancerAvailability.Available,
                Location = "TP. Hồ Chí Minh",
                PortfolioUrl = "https://github.com/example/khoi-portfolio"
            });
            Candidates.Add(new FreelanceCandidateModel
            {
                FullName = "Vũ Thị Ngọc Anh",
                Headline = ".NET Backend Developer",
                Skills = new() { "C#", "ASP.NET Core", "SQL Server", "Azure" },
                ExperienceYears = 3,
                Rating = 4.6,
                CompletedProjects = 8,
                Availability = FreelancerAvailability.Available,
                Location = "Hà Nội"
            });
            Candidates.Add(new FreelanceCandidateModel
            {
                FullName = "Trịnh Anh Tuấn",
                Headline = "Mobile Developer (Flutter/Firebase)",
                Skills = new() { "Flutter", "Firebase", "Dart" },
                ExperienceYears = 2,
                Rating = 4.3,
                CompletedProjects = 5,
                Availability = FreelancerAvailability.Busy,
                Location = "Đà Nẵng"
            });
            Candidates.Add(new FreelanceCandidateModel
            {
                FullName = "Nguyễn Hải Đăng",
                Headline = "Open source contributor, chuyên hệ thống phân tán",
                Skills = new() { "Go", "Kubernetes", "gRPC" },
                ExperienceYears = 5,
                Rating = 4.9,
                CompletedProjects = 20,
                Availability = FreelancerAvailability.Unavailable,
                Location = "Remote"
            });
            Candidates.Add(new FreelanceCandidateModel
            {
                FullName = "Bùi Thanh Trúc",
                Headline = "Frontend Developer mới ra trường",
                Skills = new() { "React", "CSS", "JavaScript" },
                ExperienceYears = 1,
                Rating = 4.1,
                CompletedProjects = 3,
                Availability = FreelancerAvailability.Available,
                Location = "TP. Hồ Chí Minh"
            });

            Companies.Clear();
            Companies.Add(new PotentialCompanyModel
            {
                CompanyName = "Kho Vận Miền Nam",
                Industry = "Logistics & Chuỗi cung ứng",
                Description = "Đang tìm đối tác nâng cấp hệ thống quản lý kho lên kiến trúc microservices, dự kiến khởi động quý sau.",
                CompanySize = "50 - 100 nhân viên",
                Location = "TP. Hồ Chí Minh",
                Website = "https://khovanmiennam.example.com",
                NeededSkills = new() { "ASP.NET Core", "SQL Server", "Docker" },
                OpenProjectsCount = 1,
                EstimatedBudget = 350_000_000m,
                Status = CompanyLeadStatus.New,
                ContactName = "Ngô Quốc Huy",
                ContactEmail = "huy.ngo@khovanmiennam.example.com"
            });
            Companies.Add(new PotentialCompanyModel
            {
                CompanyName = "HealthTech Startup",
                Industry = "Y tế số",
                Description = "Startup đặt lịch khám bệnh muốn thuê ngoài đội phát triển mobile để mở rộng tính năng cho app hiện có.",
                CompanySize = "10 - 50 nhân viên",
                Location = "Hà Nội",
                Website = "https://healthtech.example.com",
                NeededSkills = new() { "Flutter", "Node.js", "MongoDB" },
                OpenProjectsCount = 2,
                EstimatedBudget = 220_000_000m,
                Status = CompanyLeadStatus.Contacted,
                ContactName = "Trần Văn Bình",
                ContactEmail = "binh.tran@healthtech.example.com",
                Note = "Đã gửi email giới thiệu năng lực công ty ngày 15/07."
            });
            Companies.Add(new PotentialCompanyModel
            {
                CompanyName = "EduBright Academy",
                Industry = "Giáo dục trực tuyến",
                Description = "Trung tâm đào tạo cần xây dựng lại nền tảng học trực tuyến, đang trao đổi điều khoản hợp tác.",
                CompanySize = "10 - 50 nhân viên",
                Location = "Đà Nẵng",
                Website = "https://edubright.example.com",
                NeededSkills = new() { "React", "ASP.NET Core", "SQL Server" },
                OpenProjectsCount = 1,
                EstimatedBudget = 180_000_000m,
                Status = CompanyLeadStatus.InTalks,
                ContactName = "Lê Thị Hoa",
                ContactEmail = "hoa.le@edubright.example.com",
                Note = "Đã họp demo lần 1, đang chờ họ duyệt báo giá."
            });
            Companies.Add(new PotentialCompanyModel
            {
                CompanyName = "GreenRetail JSC",
                Industry = "Bán lẻ",
                Description = "Chuỗi bán lẻ muốn số hoá quy trình quản lý hàng tồn kho tại 30 chi nhánh.",
                CompanySize = "100 - 300 nhân viên",
                Location = "TP. Hồ Chí Minh",
                Website = "https://greenretail.example.com",
                NeededSkills = new() { "C#", "SQL Server", "Azure" },
                OpenProjectsCount = 1,
                EstimatedBudget = 500_000_000m,
                Status = CompanyLeadStatus.New,
                ContactName = "Phạm Minh Tuấn",
                ContactEmail = "tuan.pham@greenretail.example.com"
            });
            Companies.Add(new PotentialCompanyModel
            {
                CompanyName = "Fintech Ocean",
                Industry = "Công nghệ tài chính",
                Description = "Đã hoàn tất hợp tác giai đoạn 1, hiện là đối tác chính thức, tiếp tục theo dõi cho các dự án mở rộng.",
                CompanySize = "50 - 100 nhân viên",
                Location = "TP. Hồ Chí Minh",
                Website = "https://fintechocean.example.com",
                NeededSkills = new() { "React", "Node.js", "PostgreSQL" },
                OpenProjectsCount = 0,
                Status = CompanyLeadStatus.Converted,
                ContactName = "Đỗ Thu Trang",
                ContactEmail = "trang.do@fintechocean.example.com",
                Note = "Khách hàng chính thức từ tháng 4, đang triển khai giai đoạn 2."
            });
            Companies.Add(new PotentialCompanyModel
            {
                CompanyName = "OldSchool Manufacturing",
                Industry = "Sản xuất",
                Description = "Đã liên hệ nhưng công ty tự phát triển nội bộ, hiện chưa có nhu cầu thuê ngoài.",
                CompanySize = "300+ nhân viên",
                Location = "Bình Dương",
                NeededSkills = new() { "Java", "Oracle" },
                OpenProjectsCount = 0,
                Status = CompanyLeadStatus.NotInterested,
                ContactName = "Vũ Thị Mai",
                ContactEmail = "mai.vu@oldschool.example.com",
                Note = "Từ chối vào 10/07, có thể liên hệ lại sau 6 tháng."
            });

            var firstProject = ProjectNeeds.FirstOrDefault();
            if (firstProject != null)
                SelectProject(firstProject);
            else
                ApplyFilter();

            RecalculateCompanyMatchScores();
            ApplyCompanyFilter();
            RaiseCompanyCounters();

            IsBusy = false;
        }

        // ===================== Ứng viên =====================

        /// <summary>Chọn dự án đang cần người, highlight chip và tính lại mức khớp kỹ năng cho toàn bộ ứng viên.</summary>
        private void SelectProject(ProjectNeedModel project)
        {
            if (project == null) return;

            foreach (var p in ProjectNeeds)
                p.IsSelected = ReferenceEquals(p, project);

            SelectedProjectNeed = project;
            RecalculateMatchScores();
            ApplyFilter();

            // Ép ItemsControl render lại để DataTrigger đọc IsSelected mới (ProjectNeedModel không có INotifyPropertyChanged)
            var current = ProjectNeeds.ToList();
            ProjectNeeds.Clear();
            foreach (var p in current)
                ProjectNeeds.Add(p);
        }

        /// <summary>Tính % khớp kỹ năng của mỗi ứng viên so với RequiredSkills của dự án đang chọn.</summary>
        private void RecalculateMatchScores()
        {
            var required = SelectedProjectNeed?.RequiredSkills;

            foreach (var c in Candidates)
            {
                if (required is not { Count: > 0 })
                {
                    c.MatchScore = 0;
                    continue;
                }

                var matched = c.Skills.Count(s => required.Contains(s, StringComparer.OrdinalIgnoreCase));
                c.MatchScore = Math.Round((double)matched / required.Count * 100, 0);
            }
        }

        private void ApplyFilter()
        {
            var query = Candidates.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(c =>
                    c.FullName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                    c.SkillsDisplay.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            if (AvailabilityFilter.HasValue)
                query = query.Where(c => c.Availability == AvailabilityFilter.Value);

            FilteredCandidates.Clear();
            foreach (var c in query.OrderByDescending(c => c.MatchScore).ThenByDescending(c => c.Rating))
                FilteredCandidates.Add(c);
        }

        private void Invite(FreelanceCandidateModel candidate)
        {
            if (candidate == null || SelectedProjectNeed == null) return;

            // TODO: gọi service POST /project-needs/{id}/invitations, gửi thông báo mời (liên kết mục 5.3 - Notification)
        }

        // ===================== Khách hàng tiềm năng (công ty) =====================

        /// <summary>Tính % khớp giữa nhu cầu công nghệ của công ty tiềm năng và năng lực hiện có của công ty mình.</summary>
        private void RecalculateCompanyMatchScores()
        {
            foreach (var c in Companies)
            {
                if (c.NeededSkills is not { Count: > 0 })
                {
                    c.MatchScore = 0;
                    continue;
                }

                var matched = c.NeededSkills.Count(s => OurCoreSkills.Contains(s, StringComparer.OrdinalIgnoreCase));
                c.MatchScore = Math.Round((double)matched / c.NeededSkills.Count * 100, 0);
            }
        }

        private void ApplyCompanyFilter()
        {
            var query = Companies.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(c =>
                    c.CompanyName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                    c.Industry?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                    c.SkillsDisplay.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            if (CompanyStatusFilter.HasValue)
                query = query.Where(c => c.Status == CompanyStatusFilter.Value);

            FilteredCompanies.Clear();
            foreach (var c in query.OrderByDescending(c => c.MatchScore).ThenByDescending(c => c.OpenProjectsCount))
                FilteredCompanies.Add(c);
        }

        private void UpdateCompanyStatus(PotentialCompanyModel company, CompanyLeadStatus status)
        {
            if (company == null) return;

            // TODO: gọi service PATCH /find/potential-companies/{id}/status
            company.Status = status;
            TouchCompany();
        }

        /// <summary>Gửi yêu cầu muốn trao đổi/hợp tác dự án tới công ty tiềm năng (liên kết mục 4.14 - Quản lý trao đổi).
        /// Nếu đang ở trạng thái "Mới gợi ý", tự động chuyển sang "Đã liên hệ" vì yêu cầu bản thân nó là một lượt tiếp cận.</summary>
        private void SendExchangeRequest(PotentialCompanyModel company)
        {
            if (company == null) return;

            // TODO: gọi service POST /find/potential-companies/{id}/exchange-requests,
            // tạo ExchangeContract nháp (mục 4.14) và bắn Notification tới công ty kia (mục 5.3).
            if (company.Status == CompanyLeadStatus.New)
                company.Status = CompanyLeadStatus.Contacted;

            TouchCompany();
        }

        /// <summary>Ép làm mới UI vì PotentialCompanyModel không implement INotifyPropertyChanged.</summary>
        private void TouchCompany()
        {
            ApplyCompanyFilter();
            RaiseCompanyCounters();

            if (SelectedCompany != null)
            {
                var updated = SelectedCompany;
                SelectedCompany = null;
                SelectedCompany = updated;
            }
        }

        private void RaiseCompanyCounters()
        {
            OnPropertyChanged(nameof(NewLeadCount));
            OnPropertyChanged(nameof(InTalksCount));
            OnPropertyChanged(nameof(ConvertedCount));
        }
    }
}