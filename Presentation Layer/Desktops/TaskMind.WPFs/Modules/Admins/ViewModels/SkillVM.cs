using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Admins.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Admins.ViewModels
{
    public class SkillVM : ViewModelBase
    {
        public ObservableCollection<SkillModel> Skills { get; } = new ObservableCollection<SkillModel>();

        // ----- Danh mục chính thức (đã duyệt), nhóm theo Category -----
        private ICollectionView _skillsView;
        public ICollectionView SkillsView
        {
            get => _skillsView;
            private set { _skillsView = value; OnPropertyChanged(); }
        }

        // ----- Đề xuất đang chờ duyệt -----
        private ICollectionView _pendingSkillsView;
        public ICollectionView PendingSkillsView
        {
            get => _pendingSkillsView;
            private set { _pendingSkillsView = value; OnPropertyChanged(); }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); SkillsView?.Refresh(); }
        }

        /// <summary>"All" | tên SkillCategory</summary>
        private string _categoryFilter = "All";
        public string CategoryFilter
        {
            get => _categoryFilter;
            set { _categoryFilter = value; OnPropertyChanged(); SkillsView?.Refresh(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        // ----- Panel thêm kỹ năng mới -----
        private bool _isAddPanelOpen;
        public bool IsAddPanelOpen
        {
            get => _isAddPanelOpen;
            set { _isAddPanelOpen = value; OnPropertyChanged(); }
        }

        private string _newSkillName;
        public string NewSkillName
        {
            get => _newSkillName;
            set { _newSkillName = value; OnPropertyChanged(); }
        }

        public Array CategoryOptions => Enum.GetValues(typeof(SkillCategory));
        public Array LevelOptions => Enum.GetValues(typeof(SkillLevel));

        private SkillCategory _newSkillCategory = SkillCategory.ProgrammingLanguage;
        public SkillCategory NewSkillCategory
        {
            get => _newSkillCategory;
            set { _newSkillCategory = value; OnPropertyChanged(); }
        }

        private SkillLevel _newSkillLevel = SkillLevel.Beginner;
        public SkillLevel NewSkillLevel
        {
            get => _newSkillLevel;
            set { _newSkillLevel = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand FilterCommand { get; }
        public ICommand ToggleAddPanelCommand { get; }
        public ICommand AddSkillCommand { get; }
        public ICommand DeleteSkillCommand { get; }
        public ICommand ApproveCommand { get; }
        public ICommand RejectCommand { get; }

        public SkillVM()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            FilterCommand = new RelayCommand(f => CategoryFilter = f as string ?? "All");
            ToggleAddPanelCommand = new RelayCommand(_ => IsAddPanelOpen = !IsAddPanelOpen);
            AddSkillCommand = new RelayCommand(_ => AddSkill());
            DeleteSkillCommand = new RelayCommand(DeleteSkill);
            ApproveCommand = new RelayCommand(Approve);
            RejectCommand = new RelayCommand(Reject);

            SkillsView = CollectionViewSource.GetDefaultView(Skills);
            SkillsView.Filter = FilterApprovedSkills;
            SkillsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(SkillModel.Category)));
            SkillsView.SortDescriptions.Add(new SortDescription(nameof(SkillModel.Name), ListSortDirection.Ascending));

            PendingSkillsView = new ListCollectionView(Skills) { Filter = s => !((SkillModel)s).IsApproved };

            _ = LoadDataAsync();
        }

        private bool FilterApprovedSkills(object obj)
        {
            if (obj is not SkillModel skill || !skill.IsApproved) return false;

            if (CategoryFilter != "All" &&
                !string.Equals(skill.Category.ToString(), CategoryFilter, StringComparison.OrdinalIgnoreCase))
                return false;

            if (!string.IsNullOrWhiteSpace(SearchText) &&
                skill.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) < 0)
                return false;

            return true;
        }

        private void AddSkill()
        {
            if (string.IsNullOrWhiteSpace(NewSkillName)) return;

            var skill = new SkillModel
            {
                Id = Guid.NewGuid().ToString("N")[..8],
                Name = NewSkillName.Trim(),
                Category = NewSkillCategory,
                Level = NewSkillLevel,
                IsApproved = true,
                CreatedDate = DateTime.Now
            };

            // TODO: gọi service POST /skills để lưu kỹ năng mới do Admin tạo
            Skills.Add(skill);

            NewSkillName = string.Empty;
            IsAddPanelOpen = false;
        }

        private void DeleteSkill(object obj)
        {
            if (obj is SkillModel skill)
            {
                // TODO: gọi service DELETE /skills/{id}
                Skills.Remove(skill);
            }
        }

        private void Approve(object obj)
        {
            if (obj is SkillModel skill)
            {
                skill.IsApproved = true;
                // TODO: gọi service PUT /skills/{id}/approve
                Touch(skill);
            }
        }

        private void Reject(object obj)
        {
            if (obj is SkillModel skill)
            {
                // TODO: gọi service DELETE hoặc PUT /skills/{id}/reject
                Skills.Remove(skill);
            }
        }

        /// <summary>SkillModel chưa implement INotifyPropertyChanged nên cần "chạm" lại item để 2 view cùng refresh.</summary>
        private void Touch(SkillModel changed)
        {
            int index = Skills.IndexOf(changed);
            if (index >= 0)
            {
                Skills.RemoveAt(index);
                Skills.Insert(index, changed);
            }
        }

        /// <summary>
        /// TODO: thay bằng gọi service/API thực tế lấy danh mục kỹ năng + đề xuất chờ duyệt.
        /// </summary>
        private async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            await Task.Delay(400);

            Skills.Clear();
            foreach (var s in new[]
            {
                new SkillModel { Id="K001", Name="C#", Category=SkillCategory.ProgrammingLanguage, Level=SkillLevel.Advanced, IsApproved=true, CreatedDate=new DateTime(2023,1,1) },
                new SkillModel { Id="K002", Name="Python", Category=SkillCategory.ProgrammingLanguage, Level=SkillLevel.Advanced, IsApproved=true, CreatedDate=new DateTime(2023,1,1) },
                new SkillModel { Id="K003", Name="JavaScript", Category=SkillCategory.ProgrammingLanguage, Level=SkillLevel.Intermediate, IsApproved=true, CreatedDate=new DateTime(2023,1,1) },
                new SkillModel { Id="K004", Name="WPF", Category=SkillCategory.Framework, Level=SkillLevel.Intermediate, IsApproved=true, CreatedDate=new DateTime(2023,2,1) },
                new SkillModel { Id="K005", Name="ASP.NET Core", Category=SkillCategory.Framework, Level=SkillLevel.Advanced, IsApproved=true, CreatedDate=new DateTime(2023,2,1) },
                new SkillModel { Id="K006", Name="React", Category=SkillCategory.Framework, Level=SkillLevel.Intermediate, IsApproved=true, CreatedDate=new DateTime(2023,2,1) },
                new SkillModel { Id="K007", Name="Giao tiếp", Category=SkillCategory.SoftSkill, Level=SkillLevel.Beginner, IsApproved=true, CreatedDate=new DateTime(2023,3,1) },
                new SkillModel { Id="K008", Name="Làm việc nhóm", Category=SkillCategory.SoftSkill, Level=SkillLevel.Beginner, IsApproved=true, CreatedDate=new DateTime(2023,3,1) },
                new SkillModel { Id="K009", Name="Git", Category=SkillCategory.Tool, Level=SkillLevel.Intermediate, IsApproved=true, CreatedDate=new DateTime(2023,3,10) },
                new SkillModel { Id="K010", Name="Docker", Category=SkillCategory.Tool, Level=SkillLevel.Advanced, IsApproved=true, CreatedDate=new DateTime(2023,3,10) },

                // Đề xuất đang chờ duyệt
                new SkillModel { Id="K011", Name="Rust", Category=SkillCategory.ProgrammingLanguage, Level=SkillLevel.Advanced, IsApproved=false, SuggestedBy="CloudBase JSC", CreatedDate=new DateTime(2026,7,10) },
                new SkillModel { Id="K012", Name="Kubernetes", Category=SkillCategory.Tool, Level=SkillLevel.Advanced, IsApproved=false, SuggestedBy="DataWise Corp", CreatedDate=new DateTime(2026,7,12) },
                new SkillModel { Id="K013", Name="Tư duy phản biện", Category=SkillCategory.SoftSkill, Level=SkillLevel.Intermediate, IsApproved=false, SuggestedBy="FUNiX Academy", CreatedDate=new DateTime(2026,7,13) },
            })
            {
                Skills.Add(s);
            }

            IsBusy = false;
        }
    }
}