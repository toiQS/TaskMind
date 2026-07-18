using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Companies.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Companies.ViewModels
{
    public class CandidateVM : ViewModelBase
    {
        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); ApplyFilter(); } }

        private ApplicationStatus? _statusFilter;
        public ApplicationStatus? StatusFilter { get => _statusFilter; set { _statusFilter = value; OnPropertyChanged(); ApplyFilter(); } }

        private CandidateSource? _sourceFilter;
        public CandidateSource? SourceFilter { get => _sourceFilter; set { _sourceFilter = value; OnPropertyChanged(); ApplyFilter(); } }

        private CandidateModel _selectedCandidate;
        public CandidateModel SelectedCandidate
        {
            get => _selectedCandidate;
            set
            {
                foreach (var c in Candidates)
                    c.IsSelected = ReferenceEquals(c, value);

                _selectedCandidate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedCandidate));
                OnPropertyChanged(nameof(HasNoSelectedCandidate));

                RefreshList();
            }
        }

        public bool HasSelectedCandidate => SelectedCandidate != null;
        public bool HasNoSelectedCandidate => SelectedCandidate == null;

        private string _responseInput;
        public string ResponseInput { get => _responseInput; set { _responseInput = value; OnPropertyChanged(); } }

        /// <summary>Toàn bộ ứng viên tải từ service.</summary>
        public ObservableCollection<CandidateModel> Candidates { get; } = new();

        /// <summary>Danh sách sau khi áp dụng tìm kiếm/lọc, dùng để bind lên View.</summary>
        public ObservableCollection<CandidateModel> FilteredCandidates { get; } = new();

        public int NewCount => Candidates.Count(c => c.Status == ApplicationStatus.New);
        public int InterviewCount => Candidates.Count(c => c.Status == ApplicationStatus.Interview);
        public int HiredCount => Candidates.Count(c => c.Status == ApplicationStatus.Hired);

        public ICommand RefreshCommand { get; }
        public ICommand OpenDetailCommand { get; }
        public ICommand CloseDetailCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand SetStatusFilterCommand { get; }
        public ICommand SetSourceFilterCommand { get; }
        public ICommand SetRatingCommand { get; }
        public ICommand SendResponseCommand { get; }
        public ICommand OpenCvCommand { get; }

        public ICommand MoveToInterviewCommand { get; }
        public ICommand OfferCommand { get; }
        public ICommand RejectCommand { get; }
        public ICommand HireCommand { get; }

        public CandidateVM()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadAsync());
            OpenDetailCommand = new RelayCommand(p => SelectedCandidate = p as CandidateModel);
            CloseDetailCommand = new RelayCommand(_ => SelectedCandidate = null);
            ClearFilterCommand = new RelayCommand(_ => { SearchText = string.Empty; StatusFilter = null; SourceFilter = null; });
            SetStatusFilterCommand = new RelayCommand(p => StatusFilter = p is ApplicationStatus s ? s : (ApplicationStatus?)null);
            SetSourceFilterCommand = new RelayCommand(p => SourceFilter = p is CandidateSource s ? s : (CandidateSource?)null);
            SetRatingCommand = new RelayCommand(p => SetRating(p));
            SendResponseCommand = new RelayCommand(_ => SendResponse(), _ => CanSendResponse());
            OpenCvCommand = new RelayCommand(p => OpenCv(p as CandidateModel));

            MoveToInterviewCommand = new RelayCommand(p => UpdateStatus(p as CandidateModel, ApplicationStatus.Interview));
            OfferCommand = new RelayCommand(p => UpdateStatus(p as CandidateModel, ApplicationStatus.Offered));
            RejectCommand = new RelayCommand(p => UpdateStatus(p as CandidateModel, ApplicationStatus.Rejected));
            HireCommand = new RelayCommand(p => UpdateStatus(p as CandidateModel, ApplicationStatus.Hired));

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            IsBusy = true;

            // TODO: gọi service GET /company/{companyId}/candidates thay cho dữ liệu mẫu bên dưới
            await Task.Delay(400);

            Candidates.Clear();

            var c1 = new CandidateModel
            {
                FullName = "Nguyễn Văn A",
                Email = "vana@example.com",
                Phone = "0901 234 567",
                AppliedJobTitle = "Backend Developer (.NET)",
                Source = CandidateSource.DirectApply,
                Status = ApplicationStatus.New,
                AppliedDate = DateTime.Now.AddDays(-1),
                MatchScore = 88,
                Skills = new() { "C#", "ASP.NET Core", "SQL Server", "REST API" },
                MatchedSkills = new() { "C#", "ASP.NET Core", "SQL Server" },
                ProjectHistory = new() { "Hệ thống ERP nội bộ (Developer)", "App quản lý kho (Developer)" },
                CvFileName = "nguyenvana_cv.pdf",
                CvUrl = "https://cdn.taskmind.vn/cv/nguyenvana_cv.pdf",
                PortfolioUrl = "https://github.com/nguyenvana",
                Rating = 4
            };
            c1.Responses.Add(new CandidateResponseModel
            {
                Content = "Cảm ơn bạn đã ứng tuyển, chúng tôi sẽ phản hồi trong 3 ngày làm việc.",
                IsFromCompany = true,
                SenderName = "Phòng Nhân sự",
                SentDate = DateTime.Now.AddHours(-20)
            });
            Candidates.Add(c1);

            Candidates.Add(new CandidateModel
            {
                FullName = "Trần Thị B",
                Email = "thib@example.com",
                Phone = "0902 345 678",
                AppliedJobTitle = "Backend Developer (.NET)",
                Source = CandidateSource.Referral,
                Status = ApplicationStatus.Interview,
                AppliedDate = DateTime.Now.AddDays(-3),
                MatchScore = 72,
                Skills = new() { "C#", "REST API", "Docker" },
                MatchedSkills = new() { "C#", "REST API" },
                CvFileName = "tranthib_cv.pdf",
                CvUrl = "https://cdn.taskmind.vn/cv/tranthib_cv.pdf",
                Rating = 3
            });

            Candidates.Add(new CandidateModel
            {
                FullName = "Phạm Thị D",
                Email = "thid@example.com",
                Phone = "0904 567 890",
                AppliedJobTitle = "Thực tập sinh QA/QC",
                Source = CandidateSource.OpenSource,
                Status = ApplicationStatus.Offered,
                AppliedDate = DateTime.Now.AddDays(-6),
                MatchScore = 91,
                Skills = new() { "Manual Testing", "Test Case Design", "Selenium" },
                MatchedSkills = new() { "Manual Testing", "Test Case Design" },
                ProjectHistory = new() { "Thư viện xử lý ảnh open source cho .NET (Contributor)" },
                CvFileName = "phamthid_cv.pdf",
                CvUrl = "https://cdn.taskmind.vn/cv/phamthid_cv.pdf",
                Rating = 5
            });

            Candidates.Add(new CandidateModel
            {
                FullName = "Lê Văn C",
                Email = "vanc@example.com",
                Phone = "0903 456 789",
                AppliedJobTitle = "Backend Developer (.NET)",
                Source = CandidateSource.Headhunt,
                Status = ApplicationStatus.Rejected,
                AppliedDate = DateTime.Now.AddDays(-2),
                MatchScore = 55,
                Skills = new() { "SQL Server", "Java" },
                MatchedSkills = new() { "SQL Server" },
                InternalNote = "Kinh nghiệm chủ yếu Java, chưa phù hợp yêu cầu .NET hiện tại.",
                Rating = 2
            });

            ApplyFilter();
            RaiseCounters();
            IsBusy = false;
        }

        private void ApplyFilter()
        {
            var query = Candidates.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(c =>
                    c.FullName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                    c.AppliedJobTitle?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                    c.SkillsDisplay.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            if (StatusFilter.HasValue) query = query.Where(c => c.Status == StatusFilter.Value);
            if (SourceFilter.HasValue) query = query.Where(c => c.Source == SourceFilter.Value);

            FilteredCandidates.Clear();
            foreach (var c in query.OrderByDescending(c => c.MatchScore).ThenByDescending(c => c.AppliedDate))
                FilteredCandidates.Add(c);
        }

        private void UpdateStatus(CandidateModel candidate, ApplicationStatus status)
        {
            if (candidate == null) return;

            // TODO: gọi service PATCH /candidates/{id}/status
            candidate.Status = status;
            Touch();
        }

        private void SetRating(object param)
        {
            if (SelectedCandidate == null) return;

            int rating = param switch
            {
                int i => i,
                string s when int.TryParse(s, out var r) => r,
                _ => 0
            };

            // TODO: gọi service PATCH /candidates/{id}/rating
            SelectedCandidate.Rating = rating;
            Touch();
        }

        private bool CanSendResponse()
            => SelectedCandidate != null && !string.IsNullOrWhiteSpace(ResponseInput);

        private void SendResponse()
        {
            if (!CanSendResponse()) return;

            // TODO: gọi service POST /candidates/{id}/responses (có thể kèm bắn Notification/email, mục 5.3)
            SelectedCandidate.Responses.Add(new CandidateResponseModel
            {
                Content = ResponseInput.Trim(),
                IsFromCompany = true,
                SenderName = "Phòng Nhân sự"
            });

            ResponseInput = string.Empty;
            Touch();
        }

        private void OpenCv(CandidateModel candidate)
        {
            if (candidate?.CvUrl == null) return;

            // TODO: mở CV bằng trình duyệt/ứng dụng mặc định, ví dụ:
            // Process.Start(new ProcessStartInfo(candidate.CvUrl) { UseShellExecute = true });
        }

        /// <summary>Ép làm mới UI vì CandidateModel không implement INotifyPropertyChanged.</summary>
        private void Touch()
        {
            ApplyFilter();
            RaiseCounters();

            if (SelectedCandidate != null)
            {
                var updated = SelectedCandidate;
                SelectedCandidate = null;
                SelectedCandidate = updated;
            }
        }

        /// <summary>Ép ItemsControl bên trái render lại container để DataTrigger đọc lại IsSelected mới nhất.</summary>
        private void RefreshList()
        {
            var current = FilteredCandidates.ToList();
            FilteredCandidates.Clear();
            foreach (var c in current)
                FilteredCandidates.Add(c);
        }

        private void RaiseCounters()
        {
            OnPropertyChanged(nameof(NewCount));
            OnPropertyChanged(nameof(InterviewCount));
            OnPropertyChanged(nameof(HiredCount));
        }
    }
}