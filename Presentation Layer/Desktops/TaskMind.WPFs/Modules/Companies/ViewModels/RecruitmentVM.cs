using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Companies.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Companies.ViewModels
{
    public class RecruitmentVM : ViewModelBase
    {
        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); ApplyFilter(); } }

        private JobStatus? _statusFilter;
        public JobStatus? StatusFilter { get => _statusFilter; set { _statusFilter = value; OnPropertyChanged(); ApplyFilter(); } }

        private EmploymentType? _typeFilter;
        public EmploymentType? TypeFilter { get => _typeFilter; set { _typeFilter = value; OnPropertyChanged(); ApplyFilter(); } }

        private JobPostingModel _selectedJob;
        public JobPostingModel SelectedJob
        {
            get => _selectedJob;
            set { _selectedJob = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasSelectedJob)); OnPropertyChanged(nameof(HasNoSelectedJob)); }
        }
        public bool HasSelectedJob => SelectedJob != null;
        public bool HasNoSelectedJob => SelectedJob == null;

        /// <summary>True khi panel "Đăng tin tuyển dụng mới" đang mở (overlay ở RecruitmentView).</summary>
        private bool _isAddingJob;
        public bool IsAddingJob { get => _isAddingJob; set { _isAddingJob = value; OnPropertyChanged(); } }

        /// <summary>ViewModel của form thêm tin, được tạo mới mỗi lần mở panel.</summary>
        private AddRecruitmentVM _addJobVM;
        public AddRecruitmentVM AddJobVM { get => _addJobVM; set { _addJobVM = value; OnPropertyChanged(); } }

        public ObservableCollection<JobPostingModel> JobPostings { get; } = new();
        public ObservableCollection<JobPostingModel> FilteredJobPostings { get; } = new();

        public int OpenCount => JobPostings.Count(j => j.Status == JobStatus.Open);
        public int TotalApplicationsCount => JobPostings.Sum(j => j.ApplicationsCount);
        public int NewApplicationsCount => JobPostings.Sum(j => j.NewApplicationsCount);

        public ICommand RefreshCommand { get; }
        public ICommand CreateJobCommand { get; }
        public ICommand OpenJobDetailCommand { get; }
        public ICommand CloseJobDetailCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand SetStatusFilterCommand { get; }
        public ICommand SetTypeFilterCommand { get; }
        public ICommand PublishJobCommand { get; }
        public ICommand CloseJobCommand { get; }

        public ICommand MoveToInterviewCommand { get; }
        public ICommand OfferCommand { get; }
        public ICommand RejectApplicationCommand { get; }
        public ICommand HireCommand { get; }

        public RecruitmentVM()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadAsync());
            CreateJobCommand = new RelayCommand(_ => CreateJob());
            OpenJobDetailCommand = new RelayCommand(p => SelectJob(p as JobPostingModel));
            CloseJobDetailCommand = new RelayCommand(_ => CloseJobDetail());
            ClearFilterCommand = new RelayCommand(_ => { SearchText = string.Empty; StatusFilter = null; TypeFilter = null; });
            SetStatusFilterCommand = new RelayCommand(p => StatusFilter = p is JobStatus s ? s : (JobStatus?)null);
            SetTypeFilterCommand = new RelayCommand(p => TypeFilter = p is EmploymentType t ? t : (EmploymentType?)null);
            PublishJobCommand = new RelayCommand(p => UpdateJobStatus(p as JobPostingModel, JobStatus.Open));
            CloseJobCommand = new RelayCommand(p => UpdateJobStatus(p as JobPostingModel, JobStatus.Closed));

            MoveToInterviewCommand = new RelayCommand(p => UpdateApplicationStatus(p as CandidateApplicationModel, ApplicationStatus.Interview));
            OfferCommand = new RelayCommand(p => UpdateApplicationStatus(p as CandidateApplicationModel, ApplicationStatus.Offered));
            RejectApplicationCommand = new RelayCommand(p => UpdateApplicationStatus(p as CandidateApplicationModel, ApplicationStatus.Rejected));
            HireCommand = new RelayCommand(p => UpdateApplicationStatus(p as CandidateApplicationModel, ApplicationStatus.Hired));

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            IsBusy = true;

            // TODO: gọi service GET /company/{companyId}/job-postings thay cho dữ liệu mẫu bên dưới
            await Task.Delay(400);

            JobPostings.Clear();

            var job1 = new JobPostingModel
            {
                Title = "Backend Developer (.NET)",
                Description = "Phát triển API cho hệ thống ERP nội bộ, làm việc với đội Technical Leader và Project Manager hiện tại.",
                Department = "Phòng Kỹ thuật",
                Location = "TP. Hồ Chí Minh",
                Level = JobLevel.Mid,
                EmploymentType = EmploymentType.FullTime,
                Status = JobStatus.Open,
                RequiredSkills = new() { "C#", "ASP.NET Core", "SQL Server", "REST API" },
                SalaryMin = 18_000_000m,
                SalaryMax = 28_000_000m,
                SalaryNegotiable = true,
                PostedDate = DateTime.Now.AddDays(-5),
                Deadline = DateTime.Now.AddDays(20),
                ViewCount = 210
            };
            job1.Applications.Add(new CandidateApplicationModel
            {
                FullName = "Nguyễn Văn A",
                Email = "vana@example.com",
                Phone = "0901 234 567",
                Status = ApplicationStatus.New,
                AppliedDate = DateTime.Now.AddDays(-1),
                MatchScore = 88,
                MatchedSkills = new() { "C#", "ASP.NET Core", "SQL Server" }
            });
            job1.Applications.Add(new CandidateApplicationModel
            {
                FullName = "Trần Thị B",
                Email = "thib@example.com",
                Phone = "0902 345 678",
                Status = ApplicationStatus.Interview,
                AppliedDate = DateTime.Now.AddDays(-3),
                MatchScore = 72,
                MatchedSkills = new() { "C#", "REST API" }
            });
            job1.Applications.Add(new CandidateApplicationModel
            {
                FullName = "Lê Văn C",
                Email = "vanc@example.com",
                Phone = "0903 456 789",
                Status = ApplicationStatus.Reviewing,
                AppliedDate = DateTime.Now.AddDays(-2),
                MatchScore = 55,
                MatchedSkills = new() { "SQL Server" }
            });
            JobPostings.Add(job1);

            var job2 = new JobPostingModel
            {
                Title = "Thực tập sinh QA/QC",
                Description = "Hỗ trợ kiểm thử các dự án nội bộ, phù hợp sinh viên năm cuối muốn tích luỹ kinh nghiệm thực tế.",
                Department = "Phòng Kỹ thuật",
                Location = "Remote",
                Level = JobLevel.Intern,
                EmploymentType = EmploymentType.Internship,
                Status = JobStatus.Open,
                RequiredSkills = new() { "Manual Testing", "Test Case Design" },
                SalaryNegotiable = true,
                PostedDate = DateTime.Now.AddDays(-10),
                Deadline = DateTime.Now.AddDays(5),
                ViewCount = 340
            };
            job2.Applications.Add(new CandidateApplicationModel
            {
                FullName = "Phạm Thị D",
                Email = "thid@example.com",
                Phone = "0904 567 890",
                Status = ApplicationStatus.Offered,
                AppliedDate = DateTime.Now.AddDays(-6),
                MatchScore = 91,
                MatchedSkills = new() { "Manual Testing", "Test Case Design" }
            });
            JobPostings.Add(job2);

            JobPostings.Add(new JobPostingModel
            {
                Title = "Project Manager",
                Description = "Đã tuyển đủ, tin được lưu trữ tham khảo.",
                Department = "Ban Điều hành",
                Location = "TP. Hồ Chí Minh",
                Level = JobLevel.Senior,
                EmploymentType = EmploymentType.FullTime,
                Status = JobStatus.Filled,
                RequiredSkills = new() { "Agile", "Project Planning", "Stakeholder Management" },
                SalaryMin = 30_000_000m,
                SalaryMax = 45_000_000m,
                PostedDate = DateTime.Now.AddMonths(-2),
                ViewCount = 560
            });

            JobPostings.Add(new JobPostingModel
            {
                Title = "Frontend Developer (React)",
                Description = "Tin đang soạn thảo, chưa công bố.",
                Department = "Phòng Kỹ thuật",
                Location = "TP. Hồ Chí Minh",
                Level = JobLevel.Junior,
                EmploymentType = EmploymentType.FullTime,
                Status = JobStatus.Draft,
                RequiredSkills = new() { "React", "TypeScript" },
                SalaryNegotiable = true,
                PostedDate = DateTime.Now
            });

            ApplyFilter();
            RaiseCounters();
            IsBusy = false;
        }

        private void ApplyFilter()
        {
            var query = JobPostings.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(j =>
                    j.Title?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                    j.SkillsDisplay.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            if (StatusFilter.HasValue) query = query.Where(j => j.Status == StatusFilter.Value);
            if (TypeFilter.HasValue) query = query.Where(j => j.EmploymentType == TypeFilter.Value);

            FilteredJobPostings.Clear();
            foreach (var j in query.OrderByDescending(j => j.PostedDate))
                FilteredJobPostings.Add(j);
        }

        /// <summary>Chọn tin để xem chi tiết, đồng thời đánh dấu IsSelected để highlight card trong danh sách trái.</summary>
        private void SelectJob(JobPostingModel job)
        {
            if (job == null) return;

            foreach (var j in JobPostings)
                j.IsSelected = ReferenceEquals(j, job);

            SelectedJob = job;
            RefreshList();
        }

        private void CloseJobDetail()
        {
            foreach (var j in JobPostings)
                j.IsSelected = false;

            SelectedJob = null;
            RefreshList();
        }

        private void UpdateJobStatus(JobPostingModel job, JobStatus status)
        {
            if (job == null) return;

            // TODO: gọi service PATCH /job-postings/{id}/status
            job.Status = status;
            Touch();
        }

        private void UpdateApplicationStatus(CandidateApplicationModel application, ApplicationStatus status)
        {
            if (application == null) return;

            // TODO: gọi service PATCH /applications/{id}/status
            application.Status = status;
            Touch();
        }

        /// <summary>Mở panel "Đăng tin tuyển dụng mới" (overlay), tạo AddRecruitmentVM mới mỗi lần mở
        /// và gán callback để nhận JobPostingModel vừa tạo hoặc đóng panel khi huỷ.</summary>
        private void CreateJob()
        {
            // Đóng panel chi tiết nếu đang mở, tránh chồng 2 overlay cùng lúc
            CloseJobDetail();

            var vm = new AddRecruitmentVM();

            vm.OnSaved = job =>
            {
                // TODO: khi có service thật, có thể gọi lại LoadAsync() thay vì chèn trực tiếp vào danh sách cục bộ
                JobPostings.Insert(0, job);
                ApplyFilter();
                RaiseCounters();

                IsAddingJob = false;
                AddJobVM = null;
            };

            vm.OnCancelled = () =>
            {
                IsAddingJob = false;
                AddJobVM = null;
            };

            AddJobVM = vm;
            IsAddingJob = true;
        }

        /// <summary>Ép làm mới UI vì các model không implement INotifyPropertyChanged.</summary>
        private void Touch()
        {
            ApplyFilter();
            RaiseCounters();
            if (SelectedJob != null)
            {
                var updated = SelectedJob;
                SelectedJob = null;
                SelectedJob = updated;
            }
        }

        /// <summary>Ép ItemsControl bên trái render lại container (để DataTrigger đọc lại IsSelected mới nhất).</summary>
        private void RefreshList()
        {
            var current = FilteredJobPostings.ToList();
            FilteredJobPostings.Clear();
            foreach (var j in current)
                FilteredJobPostings.Add(j);
        }

        private void RaiseCounters()
        {
            OnPropertyChanged(nameof(OpenCount));
            OnPropertyChanged(nameof(TotalApplicationsCount));
            OnPropertyChanged(nameof(NewApplicationsCount));
        }
    }
}