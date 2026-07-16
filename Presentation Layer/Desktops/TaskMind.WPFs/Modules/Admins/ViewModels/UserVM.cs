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
    public class UserVM : ViewModelBase
    {
        /// <summary>
        /// Callback điều hướng do AdminNavigationVM truyền vào, dùng để thay thế toàn bộ
        /// AdminCurrentView (vd. chuyển sang DetailUserVM) thay vì hiển thị overlay.
        /// Có thể null khi UserVM được tạo ở design-time.
        /// </summary>
        private readonly Action<object> _navigate;

        public ObservableCollection<UserModel> Users { get; } = new ObservableCollection<UserModel>();

        private ICollectionView _usersView;
        public ICollectionView UsersView
        {
            get => _usersView;
            private set { _usersView = value; OnPropertyChanged(); }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); UsersView?.Refresh(); }
        }

        /// <summary>"All" | "Active" | "Locked" | "Banned"</summary>
        private string _statusFilter = "All";
        public string StatusFilter
        {
            get => _statusFilter;
            set { _statusFilter = value; OnPropertyChanged(); UsersView?.Refresh(); }
        }

        /// <summary>"All" | tên UserType</summary>
        private string _typeFilter = "All";
        public string TypeFilter
        {
            get => _typeFilter;
            set { _typeFilter = value; OnPropertyChanged(); UsersView?.Refresh(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand FilterStatusCommand { get; }
        public ICommand FilterTypeCommand { get; }
        public ICommand ToggleLockCommand { get; }
        public ICommand ToggleBanCommand { get; }
        public ICommand ViewDetailCommand { get; }

        /// <summary>Constructor mặc định (dùng khi thiết kế XAML / không cần điều hướng).</summary>
        public UserVM() : this(null) { }

        /// <summary>
        /// navigate: callback do AdminNavigationVM cung cấp để thay thế AdminCurrentView,
        /// dùng khi mở DetailUserView như một trang độc lập.
        /// </summary>
        public UserVM(Action<object> navigate)
        {
            _navigate = navigate;

            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            FilterStatusCommand = new RelayCommand(f => StatusFilter = f as string ?? "All");
            FilterTypeCommand = new RelayCommand(f => TypeFilter = f as string ?? "All");
            ToggleLockCommand = new RelayCommand(ToggleLock);
            ToggleBanCommand = new RelayCommand(ToggleBan);
            ViewDetailCommand = new RelayCommand(ViewDetail);

            UsersView = CollectionViewSource.GetDefaultView(Users);
            UsersView.Filter = FilterUsers;

            _ = LoadDataAsync();
        }

        private bool FilterUsers(object obj)
        {
            if (obj is not UserModel user) return false;

            if (StatusFilter != "All" &&
                !string.Equals(user.Status.ToString(), StatusFilter, StringComparison.OrdinalIgnoreCase))
                return false;

            if (TypeFilter != "All" &&
                !string.Equals(user.Type.ToString(), TypeFilter, StringComparison.OrdinalIgnoreCase))
                return false;

            if (!string.IsNullOrWhiteSpace(SearchText) &&
                user.FullName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) < 0 &&
                user.Email.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) < 0)
                return false;

            return true;
        }

        private void ToggleLock(object obj)
        {
            if (obj is UserModel user && user.Status != UserAccountStatus.Banned)
            {
                user.Status = user.Status == UserAccountStatus.Locked
                    ? UserAccountStatus.Active
                    : UserAccountStatus.Locked;
                // TODO: gọi service PUT /users/{id}/lock hoặc /unlock
                Touch(user);
            }
        }

        private void ToggleBan(object obj)
        {
            if (obj is UserModel user)
            {
                user.Status = user.Status == UserAccountStatus.Banned
                    ? UserAccountStatus.Active
                    : UserAccountStatus.Banned;
                // TODO: gọi service PUT /users/{id}/ban hoặc /unban
                Touch(user);
            }
        }

        /// <summary>
        /// Điều hướng sang DetailUserVM như một trang độc lập (thay thế toàn bộ nội dung),
        /// thay vì hiển thị overlay. Khi bấm "Quay lại" ở DetailUserView, callback onBack
        /// sẽ điều hướng ngược lại về chính UserVM hiện tại (giữ nguyên filter/search).
        /// </summary>
        private void ViewDetail(object obj)
        {
            if (obj is UserModel user && _navigate != null)
            {
                var detailVM = new DetailUserVM(user.Id, () => _navigate(this));
                _navigate(detailVM);
            }
        }

        /// <summary>UserModel chưa implement INotifyPropertyChanged nên cần "chạm" lại item để UI + filter cập nhật.</summary>
        private void Touch(UserModel changed)
        {
            int index = Users.IndexOf(changed);
            if (index >= 0)
            {
                Users.RemoveAt(index);
                Users.Insert(index, changed);
            }
        }

        /// <summary>
        /// TODO: thay bằng gọi service/API thực tế lấy danh sách người dùng.
        /// </summary>
        private async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            await Task.Delay(400);

            Users.Clear();
            foreach (var u in new[]
            {
                new UserModel { Id="U001", FullName="Trần Thị Bích", Email="bich.tran@gmail.com", Type=UserType.Student, Status=UserAccountStatus.Active, JoinedDate=new DateTime(2025,9,1), LastActiveDate=new DateTime(2026,7,13), SkillCount=6, ProjectCount=3 },
                new UserModel { Id="U002", FullName="Lê Minh Khoa", Email="khoa.le@outlook.com", Type=UserType.JobSeeker, Status=UserAccountStatus.Active, JoinedDate=new DateTime(2024,11,20), LastActiveDate=new DateTime(2026,7,12), SkillCount=9, ProjectCount=7 },
                new UserModel { Id="U003", FullName="Phạm Gia Huy", Email="huy.pham@dev.io", Type=UserType.OssContributor, Status=UserAccountStatus.Active, JoinedDate=new DateTime(2023,5,3), LastActiveDate=new DateTime(2026,7,14), SkillCount=14, ProjectCount=21 },
                new UserModel { Id="U004", FullName="Ngô Thanh Tùng", Email="tung.ngo@gmail.com", Type=UserType.Student, Status=UserAccountStatus.Locked, JoinedDate=new DateTime(2025,3,15), LastActiveDate=new DateTime(2026,6,1), SkillCount=2, ProjectCount=1 },
                new UserModel { Id="U005", FullName="Đặng Hải Yến", Email="yen.dang@gmail.com", Type=UserType.JobSeeker, Status=UserAccountStatus.Active, JoinedDate=new DateTime(2026,1,8), LastActiveDate=new DateTime(2026,7,10), SkillCount=5, ProjectCount=2 },
                new UserModel { Id="U006", FullName="Vũ Đức Anh", Email="anh.vu@spam.net", Type=UserType.Student, Status=UserAccountStatus.Banned, JoinedDate=new DateTime(2026,4,2), LastActiveDate=new DateTime(2026,5,20), SkillCount=0, ProjectCount=0 },
            })
            {
                Users.Add(u);
            }

            IsBusy = false;
        }
    }
}