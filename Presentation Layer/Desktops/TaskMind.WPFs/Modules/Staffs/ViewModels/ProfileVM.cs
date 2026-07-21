using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Staffs.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Staffs.ViewModels
{
    public class ProfileVM : ViewModelBase
    {
        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        private bool _isEditing;
        public bool IsEditing { get => _isEditing; set { _isEditing = value; OnPropertyChanged(); } }

        private ProfileModel _profile;
        public ProfileModel Profile
        {
            get => _profile;
            set { _profile = value; OnPropertyChanged(); }
        }

        /// <summary>Bản sao dùng khi Huỷ chỉnh sửa, khôi phục lại dữ liệu gốc.</summary>
        private ProfileModel _backup;

        private string _errorMessage;
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); } }

        // ===== Input tạm để thêm mới các mục dạng danh sách =====
        private string _skillInput;
        public string SkillInput { get => _skillInput; set { _skillInput = value; OnPropertyChanged(); } }

        private SkillLevel _skillLevelInput = SkillLevel.Basic;
        public SkillLevel SkillLevelInput { get => _skillLevelInput; set { _skillLevelInput = value; OnPropertyChanged(); } }

        private string _socialUrlInput;
        public string SocialUrlInput { get => _socialUrlInput; set { _socialUrlInput = value; OnPropertyChanged(); } }

        private SocialPlatform _socialPlatformInput = SocialPlatform.GitHub;
        public SocialPlatform SocialPlatformInput { get => _socialPlatformInput; set { _socialPlatformInput = value; OnPropertyChanged(); } }

        // ===== Danh sách bind lên UI =====
        public ObservableCollection<SocialLinkModel> SocialLinks { get; } = new();
        public ObservableCollection<EducationHistoryModel> EducationHistory { get; } = new();
        public ObservableCollection<WorkExperienceModel> WorkHistory { get; } = new();
        public ObservableCollection<SkillItemModel> Skills { get; } = new();
        public ObservableCollection<ProjectHistorySummary> ProjectHistory { get; } = new();

        public ICommand RefreshCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public ICommand AddSkillCommand { get; }
        public ICommand RemoveSkillCommand { get; }

        public ICommand AddSocialLinkCommand { get; }
        public ICommand RemoveSocialLinkCommand { get; }

        public ICommand AddEducationCommand { get; }
        public ICommand RemoveEducationCommand { get; }

        public ICommand AddWorkExperienceCommand { get; }
        public ICommand RemoveWorkExperienceCommand { get; }

        public ProfileVM()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadAsync());
            EditCommand = new RelayCommand(_ => StartEdit());
            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            CancelCommand = new RelayCommand(_ => CancelEdit());

            AddSkillCommand = new RelayCommand(_ => AddSkill());
            RemoveSkillCommand = new RelayCommand(p => RemoveSkill(p as SkillItemModel));

            AddSocialLinkCommand = new RelayCommand(_ => AddSocialLink());
            RemoveSocialLinkCommand = new RelayCommand(p => RemoveSocialLink(p as SocialLinkModel));

            AddEducationCommand = new RelayCommand(_ => AddEducation());
            RemoveEducationCommand = new RelayCommand(p => RemoveEducation(p as EducationHistoryModel));

            AddWorkExperienceCommand = new RelayCommand(_ => AddWorkExperience());
            RemoveWorkExperienceCommand = new RelayCommand(p => RemoveWorkExperience(p as WorkExperienceModel));

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            IsBusy = true;

            // TODO: gọi service GET /me/profile (kết hợp GET /company/{companyId}/staffs/{staffId} để
            // đồng bộ Position/Department/Status/JoinDate từ StaffModel bên Companies) thay cho dữ liệu mẫu.
            await Task.Delay(400);

            Profile = new ProfileModel
            {
                FullName = "Lê Thị Hoa",
                Email = "hoalt@taskmind.vn",
                Phone = "0911 222 333",
                DateOfBirth = new DateTime(1996, 5, 12),
                Bio = "Technical Leader yêu thích kiến trúc hệ thống và mentoring các bạn Developer trẻ.",
                CompanyName = "TaskMind Software JSC",
                Position = "Technical Leader",
                Department = "Phòng Kỹ thuật",
                Status = StaffStatus.Active,
                JoinDate = DateTime.Now.AddYears(-2),
                Visibility = ProfileVisibility.CompanyOnly,
                ShowContactInfo = true,
                ShowProjectHistory = true
            };

            Profile.SocialLinks.Add(new SocialLinkModel { Platform = SocialPlatform.GitHub, Url = "https://github.com/hoale" });
            Profile.SocialLinks.Add(new SocialLinkModel { Platform = SocialPlatform.LinkedIn, Url = "https://linkedin.com/in/hoale" });

            Profile.EducationHistory.Add(new EducationHistoryModel
            {
                SchoolName = "Đại học Bách Khoa TP.HCM",
                Major = "Kỹ thuật phần mềm",
                StartYear = 2014,
                EndYear = 2018
            });

            Profile.WorkHistory.Add(new WorkExperienceModel
            {
                CompanyName = "FPT Software",
                Position = "Software Engineer",
                StartDate = new DateTime(2018, 8, 1),
                EndDate = new DateTime(2023, 6, 30),
                Description = "Phát triển hệ thống ERP cho khách hàng Nhật Bản."
            });

            Profile.Skills.Add(new SkillItemModel { Name = "C#", Level = SkillLevel.Expert, EndorsedBy = new() { "Trần Văn Bình", "Đỗ Thu Trang" } });
            Profile.Skills.Add(new SkillItemModel { Name = "ASP.NET Core", Level = SkillLevel.Expert, EndorsedBy = new() { "Trần Văn Bình" } });
            Profile.Skills.Add(new SkillItemModel { Name = "Kiến trúc hệ thống", Level = SkillLevel.Proficient });

            Profile.ProjectHistory.Add(new ProjectHistorySummary
            {
                ProjectName = "Hệ thống ERP nội bộ",
                RoleName = "Technical Leader",
                StartDate = DateTime.Now.AddMonths(-2),
                IsCompleted = false
            });
            Profile.ProjectHistory.Add(new ProjectHistorySummary
            {
                ProjectName = "Website thương mại điện tử ABC",
                RoleName = "Technical Leader",
                StartDate = DateTime.Now.AddMonths(-1),
                IsCompleted = false
            });
            Profile.ProjectHistory.Add(new ProjectHistorySummary
            {
                ProjectName = "App quản lý kho",
                RoleName = "Developer",
                StartDate = DateTime.Now.AddMonths(-8),
                EndDate = DateTime.Now.AddDays(-10),
                IsCompleted = true
            });

            SyncCollectionsFromProfile();
            IsBusy = false;
        }

        /// <summary>Đổ dữ liệu từ Profile (nguồn gốc) vào các ObservableCollection dùng để bind UI.</summary>
        private void SyncCollectionsFromProfile()
        {
            SocialLinks.Clear();
            foreach (var s in Profile.SocialLinks) SocialLinks.Add(s);

            EducationHistory.Clear();
            foreach (var e in Profile.EducationHistory) EducationHistory.Add(e);

            WorkHistory.Clear();
            foreach (var w in Profile.WorkHistory) WorkHistory.Add(w);

            Skills.Clear();
            foreach (var sk in Profile.Skills) Skills.Add(sk);

            ProjectHistory.Clear();
            foreach (var p in Profile.ProjectHistory) ProjectHistory.Add(p);
        }

        private void StartEdit()
        {
            ErrorMessage = string.Empty;

            // Lưu bản sao (nông) để khôi phục khi Huỷ; các danh sách con giữ nguyên tham chiếu hiện tại.
            _backup = new ProfileModel
            {
                Id = Profile.Id,
                FullName = Profile.FullName,
                AvatarUrl = Profile.AvatarUrl,
                DateOfBirth = Profile.DateOfBirth,
                Email = Profile.Email,
                Phone = Profile.Phone,
                Bio = Profile.Bio,
                SocialLinks = Profile.SocialLinks.ToList(),
                EducationHistory = Profile.EducationHistory.ToList(),
                WorkHistory = Profile.WorkHistory.ToList(),
                Skills = Profile.Skills.ToList(),
                ProjectHistory = Profile.ProjectHistory,
                CompanyName = Profile.CompanyName,
                Position = Profile.Position,
                Department = Profile.Department,
                Status = Profile.Status,
                JoinDate = Profile.JoinDate,
                LeftDate = Profile.LeftDate,
                Visibility = Profile.Visibility,
                ShowContactInfo = Profile.ShowContactInfo,
                ShowProjectHistory = Profile.ShowProjectHistory
            };

            IsEditing = true;
        }

        private async Task SaveAsync()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Profile.FullName))
            {
                ErrorMessage = "Họ và tên không được để trống.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Profile.Email))
            {
                ErrorMessage = "Vui lòng nhập email liên hệ.";
                return;
            }

            IsBusy = true;

            Profile.SocialLinks = SocialLinks.ToList();
            Profile.EducationHistory = EducationHistory.ToList();
            Profile.WorkHistory = WorkHistory.ToList();
            Profile.Skills = Skills.ToList();

            // TODO: gọi service PUT /me/profile thay cho việc lưu cục bộ hiện tại. Chỉ gửi các trường
            // nhân sự được phép tự sửa — KHÔNG gửi Position/Department/Status/JoinDate (do Admin company quản lý).
            await Task.Delay(500);

            IsBusy = false;
            IsEditing = false;
        }

        private void CancelEdit()
        {
            if (_backup != null)
            {
                Profile = _backup;
                SyncCollectionsFromProfile();
            }

            ErrorMessage = string.Empty;
            IsEditing = false;
        }

        // ===== Kỹ năng cá nhân (mục 4.3) =====
        private void AddSkill()
        {
            var name = SkillInput?.Trim();
            if (string.IsNullOrWhiteSpace(name)) return;

            if (!Skills.Any(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase)))
                Skills.Add(new SkillItemModel { Name = name, Level = SkillLevelInput });

            SkillInput = string.Empty;
        }

        private void RemoveSkill(SkillItemModel skill)
        {
            if (skill == null) return;
            Skills.Remove(skill);
        }

        // ===== Liên kết mạng xã hội/portfolio (mục 4.2) =====
        private void AddSocialLink()
        {
            var url = SocialUrlInput?.Trim();
            if (string.IsNullOrWhiteSpace(url)) return;

            var existing = SocialLinks.FirstOrDefault(s => s.Platform == SocialPlatformInput);
            if (existing != null)
            {
                // Đã có liên kết cho nền tảng này -> cập nhật URL thay vì tạo trùng
                existing.Url = url;
                var current = SocialLinks.ToList();
                SocialLinks.Clear();
                foreach (var s in current) SocialLinks.Add(s);
            }
            else
            {
                SocialLinks.Add(new SocialLinkModel { Platform = SocialPlatformInput, Url = url });
            }

            SocialUrlInput = string.Empty;
        }

        private void RemoveSocialLink(SocialLinkModel link)
        {
            if (link == null) return;
            SocialLinks.Remove(link);
        }

        // ===== Học vấn (mục 4.2) =====
        private void AddEducation()
        {
            EducationHistory.Add(new EducationHistoryModel
            {
                SchoolName = "Trường mới",
                Major = "Chuyên ngành",
                StartYear = DateTime.Now.Year
            });
        }

        private void RemoveEducation(EducationHistoryModel item)
        {
            if (item == null) return;
            EducationHistory.Remove(item);
        }

        // ===== Kinh nghiệm làm việc trước đó (mục 4.2) =====
        private void AddWorkExperience()
        {
            WorkHistory.Add(new WorkExperienceModel
            {
                CompanyName = "Công ty mới",
                Position = "Vị trí",
                StartDate = DateTime.Now
            });
        }

        private void RemoveWorkExperience(WorkExperienceModel item)
        {
            if (item == null) return;
            WorkHistory.Remove(item);
        }
    }
}