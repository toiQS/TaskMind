using System.Collections.ObjectModel;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Companies.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Companies.ViewModels
{
    public class AddRecruitmentVM : ViewModelBase
    {
        private string _title;
        public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }

        private string _description;
        public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }

        private string _department;
        public string Department { get => _department; set { _department = value; OnPropertyChanged(); } }

        private string _location;
        public string Location { get => _location; set { _location = value; OnPropertyChanged(); } }

        private JobLevel _level = JobLevel.Junior;
        public JobLevel Level { get => _level; set { _level = value; OnPropertyChanged(); } }

        private EmploymentType _employmentType = EmploymentType.FullTime;
        public EmploymentType EmploymentType { get => _employmentType; set { _employmentType = value; OnPropertyChanged(); } }

        private decimal? _salaryMin;
        public decimal? SalaryMin { get => _salaryMin; set { _salaryMin = value; OnPropertyChanged(); } }

        private decimal? _salaryMax;
        public decimal? SalaryMax { get => _salaryMax; set { _salaryMax = value; OnPropertyChanged(); } }

        private bool _salaryNegotiable;
        public bool SalaryNegotiable { get => _salaryNegotiable; set { _salaryNegotiable = value; OnPropertyChanged(); } }

        private DateTime? _deadline;
        public DateTime? Deadline { get => _deadline; set { _deadline = value; OnPropertyChanged(); } }

        private string _skillInput;
        public string SkillInput { get => _skillInput; set { _skillInput = value; OnPropertyChanged(); } }

        /// <summary>Danh sách kỹ năng yêu cầu dạng tag, tham chiếu danh mục kỹ năng chuẩn hoá (mục 4.15).</summary>
        public ObservableCollection<string> RequiredSkills { get; } = new();

        private string _errorMessage;
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); } }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        public ICommand AddSkillCommand { get; }
        public ICommand RemoveSkillCommand { get; }
        public ICommand SaveDraftCommand { get; }
        public ICommand PublishCommand { get; }
        public ICommand CancelCommand { get; }

        /// <summary>RecruitmentVM gán 2 callback này khi mở form, để nhận kết quả và đóng overlay.</summary>
        public Action<JobPostingModel> OnSaved { get; set; }
        public Action OnCancelled { get; set; }

        public AddRecruitmentVM()
        {
            AddSkillCommand = new RelayCommand(_ => AddSkill());
            RemoveSkillCommand = new RelayCommand(p => RemoveSkill(p as string));
            SaveDraftCommand = new RelayCommand(async _ => await SaveAsync(JobStatus.Draft));
            PublishCommand = new RelayCommand(async _ => await SaveAsync(JobStatus.Open));
            CancelCommand = new RelayCommand(_ => OnCancelled?.Invoke());
        }

        private void AddSkill()
        {
            var skill = SkillInput?.Trim();
            if (string.IsNullOrWhiteSpace(skill)) return;

            if (!RequiredSkills.Contains(skill, StringComparer.OrdinalIgnoreCase))
                RequiredSkills.Add(skill);

            SkillInput = string.Empty;
        }

        private void RemoveSkill(string skill)
        {
            if (skill == null) return;
            RequiredSkills.Remove(skill);
        }

        private bool Validate()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Title))
            {
                ErrorMessage = "Vui lòng nhập tên vị trí tuyển dụng.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Department))
            {
                ErrorMessage = "Vui lòng nhập phòng ban.";
                return false;
            }

            if (RequiredSkills.Count == 0)
            {
                ErrorMessage = "Vui lòng thêm ít nhất 1 kỹ năng yêu cầu.";
                return false;
            }

            if (!SalaryNegotiable && SalaryMin.HasValue && SalaryMax.HasValue && SalaryMin > SalaryMax)
            {
                ErrorMessage = "Mức lương tối thiểu không được lớn hơn mức lương tối đa.";
                return false;
            }

            return true;
        }

        private async Task SaveAsync(JobStatus status)
        {
            if (!Validate()) return;

            IsBusy = true;

            var job = new JobPostingModel
            {
                Title = Title.Trim(),
                Description = Description?.Trim(),
                Department = Department.Trim(),
                Location = Location?.Trim(),
                Level = Level,
                EmploymentType = EmploymentType,
                Status = status,
                RequiredSkills = RequiredSkills.ToList(),
                SalaryMin = SalaryNegotiable ? null : SalaryMin,
                SalaryMax = SalaryNegotiable ? null : SalaryMax,
                SalaryNegotiable = SalaryNegotiable,
                Deadline = Deadline,
                PostedDate = DateTime.Now
            };

            // TODO: gọi service POST /job-postings thay cho việc thêm trực tiếp vào danh sách cục bộ ở RecruitmentVM
            await Task.Delay(400);

            IsBusy = false;
            OnSaved?.Invoke(job);
        }
    }
}