using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Companies.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Companies.ViewModels
{
    public class AddStaffVM : ViewModelBase
    {
        private AddStaffSourceMode _sourceMode = AddStaffSourceMode.FromCandidate;
        public AddStaffSourceMode SourceMode
        {
            get => _sourceMode;
            set { _sourceMode = value; OnPropertyChanged(); }
        }

        // ===== Danh sách ứng viên đã tuyển (mock, thay bằng service khi có backend) =====
        public ObservableCollection<HiredCandidateOption> HiredCandidates { get; } = new();

        private HiredCandidateOption _selectedCandidate;
        public HiredCandidateOption SelectedCandidate
        {
            get => _selectedCandidate;
            set
            {
                foreach (var c in HiredCandidates)
                    c.IsSelected = ReferenceEquals(c, value);

                _selectedCandidate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedCandidate));

                if (value != null)
                {
                    FullName = value.FullName;
                    Email = value.Email;
                    Skills.Clear();
                    foreach (var s in value.Skills)
                        Skills.Add(s);
                }

                RefreshCandidateList();
            }
        }
        public bool HasSelectedCandidate => SelectedCandidate != null;

        // ===== Thông tin nhân sự (dùng chung cho cả 2 chế độ) =====
        private string _fullName;
        public string FullName { get => _fullName; set { _fullName = value; OnPropertyChanged(); } }

        private string _email;
        public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }

        private string _phone;
        public string Phone { get => _phone; set { _phone = value; OnPropertyChanged(); } }

        private string _position;
        public string Position { get => _position; set { _position = value; OnPropertyChanged(); } }

        private string _department;
        public string Department { get => _department; set { _department = value; OnPropertyChanged(); } }

        private DateTime _joinDate = DateTime.Now;
        public DateTime JoinDate { get => _joinDate; set { _joinDate = value; OnPropertyChanged(); } }

        private string _skillInput;
        public string SkillInput { get => _skillInput; set { _skillInput = value; OnPropertyChanged(); } }

        public ObservableCollection<string> Skills { get; } = new();

        private string _note;
        public string Note { get => _note; set { _note = value; OnPropertyChanged(); } }

        private string _errorMessage;
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); } }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        public ICommand SelectSourceModeCommand { get; }
        public ICommand SelectCandidateCommand { get; }
        public ICommand AddSkillCommand { get; }
        public ICommand RemoveSkillCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        /// <summary>StaffVM gán 2 callback này khi mở form, để nhận StaffModel vừa tạo hoặc đóng panel khi huỷ.</summary>
        public Action<StaffModel> OnSaved { get; set; }
        public Action OnCancelled { get; set; }

        public AddStaffVM()
        {
            SelectSourceModeCommand = new RelayCommand(p => ChangeSourceMode(p));
            SelectCandidateCommand = new RelayCommand(p => SelectedCandidate = p as HiredCandidateOption);
            AddSkillCommand = new RelayCommand(_ => AddSkill());
            RemoveSkillCommand = new RelayCommand(p => RemoveSkill(p as string));
            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            CancelCommand = new RelayCommand(_ => OnCancelled?.Invoke());

            _ = LoadHiredCandidatesAsync();
        }

        private async Task LoadHiredCandidatesAsync()
        {
            // TODO: gọi service GET /company/{companyId}/candidates?status=Hired thay cho dữ liệu mẫu bên dưới
            // (nguồn dữ liệu tương ứng CandidateVM/RecruitmentVM đã lọc theo ApplicationStatus.Hired).
            await Task.Delay(200);

            HiredCandidates.Clear();
            HiredCandidates.Add(new HiredCandidateOption
            {
                FullName = "Phạm Thị D",
                Email = "thid@example.com",
                AppliedJobTitle = "Thực tập sinh QA/QC",
                Skills = new() { "Manual Testing", "Test Case Design" }
            });
            HiredCandidates.Add(new HiredCandidateOption
            {
                FullName = "Nguyễn Văn A",
                Email = "vana@example.com",
                AppliedJobTitle = "Backend Developer (.NET)",
                Skills = new() { "C#", "ASP.NET Core", "SQL Server" }
            });
        }

        private void ChangeSourceMode(object param)
        {
            SourceMode = param is AddStaffSourceMode m ? m : AddStaffSourceMode.FromCandidate;

            // Đổi chế độ thì làm sạch dữ liệu đã điền để tránh lẫn giữa 2 luồng
            SelectedCandidate = null;
            FullName = string.Empty;
            Email = string.Empty;
            Skills.Clear();
            ErrorMessage = string.Empty;
        }

        private void AddSkill()
        {
            var skill = SkillInput?.Trim();
            if (string.IsNullOrWhiteSpace(skill)) return;

            if (!Skills.Contains(skill, StringComparer.OrdinalIgnoreCase))
                Skills.Add(skill);

            SkillInput = string.Empty;
        }

        private void RemoveSkill(string skill)
        {
            if (skill == null) return;
            Skills.Remove(skill);
        }

        private bool Validate()
        {
            ErrorMessage = string.Empty;

            if (SourceMode == AddStaffSourceMode.FromCandidate && SelectedCandidate == null)
            {
                ErrorMessage = "Vui lòng chọn một ứng viên đã tuyển.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(FullName))
            {
                ErrorMessage = "Vui lòng nhập họ tên nhân sự.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Vui lòng nhập email.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Position))
            {
                ErrorMessage = "Vui lòng nhập chức danh.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Department))
            {
                ErrorMessage = "Vui lòng nhập phòng ban.";
                return false;
            }

            return true;
        }

        private async Task SaveAsync()
        {
            if (!Validate()) return;

            IsBusy = true;

            var staff = new StaffModel
            {
                FullName = FullName.Trim(),
                Email = Email.Trim(),
                Phone = Phone?.Trim(),
                Position = Position.Trim(),
                Department = Department.Trim(),
                Status = StaffStatus.Active,
                JoinDate = JoinDate,
                Skills = Skills.ToList(),
                SourceCandidateName = SourceMode == AddStaffSourceMode.FromCandidate && SelectedCandidate != null
                    ? $"{SelectedCandidate.FullName} (ứng viên tin \"{SelectedCandidate.AppliedJobTitle}\")"
                    : null,
                Note = Note?.Trim()
            };

            // TODO: gọi service POST /staffs thay cho việc thêm trực tiếp vào danh sách cục bộ ở StaffVM
            await Task.Delay(400);

            IsBusy = false;
            OnSaved?.Invoke(staff);
        }

        /// <summary>Ép ItemsControl render lại container để DataTrigger đọc lại IsSelected mới nhất
        /// (HiredCandidateOption không implement INotifyPropertyChanged).</summary>
        private void RefreshCandidateList()
        {
            var current = HiredCandidates.ToList();
            HiredCandidates.Clear();
            foreach (var c in current)
                HiredCandidates.Add(c);
        }
    }
}