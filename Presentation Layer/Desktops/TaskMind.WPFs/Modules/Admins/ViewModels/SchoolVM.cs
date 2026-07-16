using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Admins.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Admins.ViewModels
{
    public class SchoolVM : ViewModelBase
    {
        /// <summary>
        /// Callback điều hướng do AdminNavigationVM truyền vào, dùng để thay thế toàn bộ
        /// AdminCurrentView (vd. chuyển sang DetailSchoolVM) thay vì hiển thị overlay.
        /// Có thể null khi SchoolVM được tạo ở design-time.
        /// </summary>
        private readonly Action<object> _navigate;

        public ObservableCollection<SchoolModel> Schools { get; } = new ObservableCollection<SchoolModel>();

        private ICollectionView _schoolsView;
        public ICollectionView SchoolsView
        {
            get => _schoolsView;
            private set { _schoolsView = value; OnPropertyChanged(); }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); SchoolsView?.Refresh(); }
        }

        /// <summary>"All" | "Pending" | "Active" | "Suspended" | "Rejected"</summary>
        private string _statusFilter = "All";
        public string StatusFilter
        {
            get => _statusFilter;
            set { _statusFilter = value; OnPropertyChanged(); SchoolsView?.Refresh(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        // ----- Panel "Thêm cơ sở đào tạo mới" (vẫn hiển thị dạng overlay/modal) -----
        private bool _isAddPanelOpen;
        public bool IsAddPanelOpen
        {
            get => _isAddPanelOpen;
            set { _isAddPanelOpen = value; OnPropertyChanged(); }
        }

        private AddSchoolVM _addSchoolVM;
        public AddSchoolVM AddSchoolVM
        {
            get => _addSchoolVM;
            private set { _addSchoolVM = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand FilterCommand { get; }
        public ICommand ApproveCommand { get; }
        public ICommand RejectCommand { get; }
        public ICommand ToggleSuspendCommand { get; }
        public ICommand OpenAddSchoolCommand { get; }
        public ICommand ViewDetailCommand { get; }

        /// <summary>Constructor mặc định (dùng khi thiết kế XAML / không cần điều hướng).</summary>
        public SchoolVM() : this(null) { }

        /// <summary>
        /// navigate: callback do AdminNavigationVM cung cấp để thay thế AdminCurrentView,
        /// dùng khi mở DetailSchoolView như một trang độc lập.
        /// </summary>
        public SchoolVM(Action<object> navigate)
        {
            _navigate = navigate;

            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            FilterCommand = new RelayCommand(f => StatusFilter = f as string ?? "All");
            ApproveCommand = new RelayCommand(Approve);
            RejectCommand = new RelayCommand(Reject);
            ToggleSuspendCommand = new RelayCommand(ToggleSuspend);
            OpenAddSchoolCommand = new RelayCommand(_ => OpenAddPanel());
            ViewDetailCommand = new RelayCommand(ViewDetail);

            SchoolsView = CollectionViewSource.GetDefaultView(Schools);
            SchoolsView.Filter = FilterSchools;

            _ = LoadDataAsync();
        }

        private bool FilterSchools(object obj)
        {
            if (obj is not SchoolModel school) return false;

            if (StatusFilter != "All" &&
                !string.Equals(school.Status.ToString(), StatusFilter, StringComparison.OrdinalIgnoreCase))
                return false;

            if (!string.IsNullOrWhiteSpace(SearchText) &&
                school.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) < 0)
                return false;

            return true;
        }

        private void Approve(object obj)
        {
            if (obj is SchoolModel school)
            {
                school.Status = SchoolStatus.Active;
                // TODO: gọi service cập nhật trạng thái cơ sở đào tạo (PUT /schools/{id}/approve)
                Touch(school);
            }
        }

        private void Reject(object obj)
        {
            if (obj is SchoolModel school)
            {
                school.Status = SchoolStatus.Rejected;
                // TODO: gọi service cập nhật trạng thái cơ sở đào tạo (PUT /schools/{id}/reject)
                Touch(school);
            }
        }

        private void ToggleSuspend(object obj)
        {
            if (obj is SchoolModel school)
            {
                school.Status = school.Status == SchoolStatus.Suspended
                    ? SchoolStatus.Active
                    : SchoolStatus.Suspended;
                // TODO: gọi service cập nhật trạng thái cơ sở đào tạo
                Touch(school);
            }
        }

        /// <summary>Mở panel thêm cơ sở đào tạo, luôn tạo mới AddSchoolVM để form trống mỗi lần mở.</summary>
        private void OpenAddPanel()
        {
            AddSchoolVM = new AddSchoolVM(OnSchoolCreated, CloseAddPanel);
            IsAddPanelOpen = true;
        }

        private void CloseAddPanel()
        {
            IsAddPanelOpen = false;
            AddSchoolVM = null;
        }

        /// <summary>Callback khi AddSchoolVM tạo cơ sở thành công: thêm vào danh sách và đóng panel.</summary>
        private void OnSchoolCreated(SchoolModel newSchool)
        {
            // TODO: sau khi service tạo cơ sở đào tạo thành công (POST /schools), có thể gọi lại
            // LoadDataAsync() để đồng bộ dữ liệu thay vì chỉ thêm vào collection tại chỗ.
            Schools.Insert(0, newSchool);
            CloseAddPanel();
        }

        /// <summary>
        /// Điều hướng sang DetailSchoolVM như một trang độc lập (thay thế toàn bộ nội dung),
        /// thay vì hiển thị overlay. Khi bấm "Quay lại" ở DetailSchoolView, callback onBack
        /// sẽ điều hướng ngược lại về chính SchoolVM hiện tại (giữ nguyên filter/search).
        /// </summary>
        private void ViewDetail(object obj)
        {
            if (obj is SchoolModel school && _navigate != null)
            {
                var detailVM = new DetailSchoolVM(school.Id, () => _navigate(this));
                _navigate(detailVM);
            }
        }

        /// <summary>
        /// SchoolModel chưa implement INotifyPropertyChanged nên cần "chạm" lại item
        /// để UI + CollectionView filter cập nhật hiển thị.
        /// </summary>
        private void Touch(SchoolModel changed)
        {
            int index = Schools.IndexOf(changed);
            if (index >= 0)
            {
                Schools.RemoveAt(index);
                Schools.Insert(index, changed);
            }
        }

        /// <summary>
        /// TODO: thay bằng gọi service/API thực tế lấy danh sách cơ sở đào tạo.
        /// </summary>
        private async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            await Task.Delay(400);

            Schools.Clear();
            foreach (var s in new[]
            {
                new SchoolModel { Id="S001", Name="Học viện Công nghệ ABC", Field="Công nghệ thông tin", Email="contact@abc.edu.vn", Package="Enterprise", Status=SchoolStatus.Active, JoinedDate=new DateTime(2023,9,1), TeacherCount=42, CourseCount=18, StudentCount=1200 },
                new SchoolModel { Id="S002", Name="Trung tâm Đào tạo CodeUp", Field="Lập trình web/mobile", Email="hello@codeup.vn", Package="Pro", Status=SchoolStatus.Pending, JoinedDate=new DateTime(2026,7,5), TeacherCount=0, CourseCount=0, StudentCount=0 },
                new SchoolModel { Id="S003", Name="FUNiX Academy", Field="Công nghệ phần mềm", Email="info@funix.edu.vn", Package="Enterprise", Status=SchoolStatus.Active, JoinedDate=new DateTime(2022,4,12), TeacherCount=65, CourseCount=30, StudentCount=3400 },
                new SchoolModel { Id="S004", Name="Trung tâm Tin học XYZ", Field="Kỹ năng CNTT cơ bản", Email="xyz@center.vn", Package="Starter", Status=SchoolStatus.Suspended, JoinedDate=new DateTime(2024,2,2), TeacherCount=8, CourseCount=5, StudentCount=180 },
                new SchoolModel { Id="S005", Name="DevMaster Institute", Field="Data & AI", Email="team@devmaster.io", Package="Pro", Status=SchoolStatus.Pending, JoinedDate=new DateTime(2026,7,12), TeacherCount=0, CourseCount=0, StudentCount=0 },
                new SchoolModel { Id="S006", Name="SmartCode School", Field="Game Development", Email="hi@smartcode.dev", Package="Starter", Status=SchoolStatus.Rejected, JoinedDate=new DateTime(2026,6,18), TeacherCount=0, CourseCount=0, StudentCount=0 },
            })
            {
                Schools.Add(s);
            }

            IsBusy = false;
        }
    }
}