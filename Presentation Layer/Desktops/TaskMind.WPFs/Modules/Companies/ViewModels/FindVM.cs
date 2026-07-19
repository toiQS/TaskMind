using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Companies.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Companies.ViewModels
{
    public class FindVM : ViewModelBase
    {
        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); ApplyFilter(); } }

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

        public ObservableCollection<SkillTrendModel> SkillTrends { get; } = new();
        public ObservableCollection<ProjectNeedModel> ProjectNeeds { get; } = new();
        public ObservableCollection<FreelanceCandidateModel> Candidates { get; } = new();
        public ObservableCollection<FreelanceCandidateModel> FilteredCandidates { get; } = new();

        public ICommand RefreshCommand { get; }
        public ICommand SelectProjectCommand { get; }
        public ICommand OpenCandidateDetailCommand { get; }
        public ICommand CloseCandidateDetailCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand SetAvailabilityFilterCommand { get; }
        public ICommand InviteCommand { get; }

        public FindVM()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadAsync());
            SelectProjectCommand = new RelayCommand(p => SelectProject(p as ProjectNeedModel));
            OpenCandidateDetailCommand = new RelayCommand(p => SelectedCandidate = p as FreelanceCandidateModel);
            CloseCandidateDetailCommand = new RelayCommand(_ => SelectedCandidate = null);
            ClearFilterCommand = new RelayCommand(_ => { SearchText = string.Empty; AvailabilityFilter = null; });
            SetAvailabilityFilterCommand = new RelayCommand(p => AvailabilityFilter = p is FreelancerAvailability a ? a : (FreelancerAvailability?)null);
            InviteCommand = new RelayCommand(p => Invite(p as FreelanceCandidateModel));

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            IsBusy = true;

            // TODO: gọi service GET /company/{companyId}/find/trends, /find/project-needs, /find/candidates
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

            var firstProject = ProjectNeeds.FirstOrDefault();
            if (firstProject != null)
                SelectProject(firstProject);
            else
                ApplyFilter();

            IsBusy = false;
        }

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
    }
}