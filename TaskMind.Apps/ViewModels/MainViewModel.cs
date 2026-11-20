using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Input;
using TaskMind.Apps.Services;
using TaskMind.Apps.Utilities;

namespace TaskMind.Apps.ViewModels;

public sealed class MainViewModel : ViewModelBase, IDisposable
{
    private const string ApiBaseUrl = "http://localhost:5258/api/v1";
    private readonly TaskMindApiClient _api = new();
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _selectedRole = "Tự nhận diện";
    private string _selectedSection = "Overview";
    private string _statusMessage = "Kết nối API TaskMind để bắt đầu.";
    private bool _isBusy;
    private bool _isAuthenticated;
    private bool _isAccountSelectionRequired;
    private string _displayName = "Khách";
    private string _accountRole = string.Empty;
    private string _profileEmail = string.Empty;
    private string _profileName = string.Empty;
    private string _profileBio = string.Empty;
    private string _projectSearch = string.Empty;
    private string _skillSearch = string.Empty;
    private int _projectCount;
    private int _skillCount;
    private string? _accessToken;

    public MainViewModel()
    {
        LoginCommand = new AsyncRelayCommand(LoginAsync, () => !IsBusy);
        LogoutCommand = new RelayCommand(_ => Logout(), _ => IsAuthenticated);
        BackToLoginCommand = new RelayCommand(_ => ReturnToCredentialEntry());
        NavigateCommand = new RelayCommand(parameter =>
        {
            if (parameter is string section)
                SelectedSection = section;
        });
        RefreshCommand = new AsyncRelayCommand(RefreshCurrentSectionAsync, () => IsAuthenticated && !IsBusy);
        SearchProjectsCommand = new AsyncRelayCommand(LoadProjectsAsync, () => IsAuthenticated && !IsBusy);
        SearchSkillsCommand = new AsyncRelayCommand(LoadSkillsAsync, () => !IsBusy);
    }

    private void ReturnToCredentialEntry()
    {
        IsAccountSelectionRequired = false;
        SelectedRole = RoleOptions[0];
        StatusMessage = "Nhập email và mật khẩu để tiếp tục.";
    }

    public ObservableCollection<ProjectItem> Projects { get; } = new();
    public ObservableCollection<SkillItem> Skills { get; } = new();
    public ObservableCollection<string> RoleOptions { get; } =
        new(["Tự nhận diện", "Admin", "AdminCompany", "AdminSchool", "Staff", "Teacher", "Student", "User"]);

    public ICommand LoginCommand { get; }
    public ICommand LogoutCommand { get; }
    public ICommand BackToLoginCommand { get; }
    public ICommand NavigateCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand SearchProjectsCommand { get; }
    public ICommand SearchSkillsCommand { get; }

    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string Password { get => _password; set => SetProperty(ref _password, value); }
    public string SelectedRole { get => _selectedRole; set => SetProperty(ref _selectedRole, value); }
    public string SelectedSection
    {
        get => _selectedSection;
        set
        {
            if (!SetProperty(ref _selectedSection, value))
                return;
            OnPropertyChanged(nameof(IsOverviewSelected));
            OnPropertyChanged(nameof(IsProjectsSelected));
            OnPropertyChanged(nameof(IsSkillsSelected));
            OnPropertyChanged(nameof(IsProfileSelected));
            OnPropertyChanged(nameof(SectionTitle));
            _ = RefreshCurrentSectionAsync();
        }
    }
    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (!SetProperty(ref _isBusy, value))
                return;
            CommandManager.InvalidateRequerySuggested();
        }
    }
    public bool IsAuthenticated
    {
        get => _isAuthenticated;
        private set
        {
            if (!SetProperty(ref _isAuthenticated, value))
                return;
            OnPropertyChanged(nameof(IsLoggedOut));
            CommandManager.InvalidateRequerySuggested();
        }
    }
    public bool IsLoggedOut => !IsAuthenticated;
    public bool IsAccountSelectionRequired
    {
        get => _isAccountSelectionRequired;
        private set
        {
            if (!SetProperty(ref _isAccountSelectionRequired, value))
                return;
            OnPropertyChanged(nameof(IsCredentialEntryStep));
            OnPropertyChanged(nameof(IsRoleSelectionStep));
        }
    }
    public bool IsCredentialEntryStep => !IsAccountSelectionRequired;
    public bool IsRoleSelectionStep => IsAccountSelectionRequired;
    public string DisplayName
    {
        get => _displayName;
        private set
        {
            if (SetProperty(ref _displayName, value))
                OnPropertyChanged(nameof(DisplayInitial));
        }
    }
    public string AccountRole { get => _accountRole; private set => SetProperty(ref _accountRole, value); }
    public string ProfileEmail { get => _profileEmail; private set => SetProperty(ref _profileEmail, value); }
    public string ProfileName { get => _profileName; private set => SetProperty(ref _profileName, value); }
    public string ProfileBio { get => _profileBio; private set => SetProperty(ref _profileBio, value); }
    public string ProjectSearch { get => _projectSearch; set => SetProperty(ref _projectSearch, value); }
    public string SkillSearch { get => _skillSearch; set => SetProperty(ref _skillSearch, value); }
    public int ProjectCount { get => _projectCount; private set => SetProperty(ref _projectCount, value); }
    public int SkillCount { get => _skillCount; private set => SetProperty(ref _skillCount, value); }
    public bool IsOverviewSelected => SelectedSection == "Overview";
    public bool IsProjectsSelected => SelectedSection == "Projects";
    public bool IsSkillsSelected => SelectedSection == "Skills";
    public bool IsProfileSelected => SelectedSection == "Profile";
    public string SectionTitle => SelectedSection switch
    {
        "Overview" => "Tổng quan",
        "Projects" => "Dự án",
        "Skills" => "Kỹ năng",
        "Profile" => "Hồ sơ của tôi",
        _ => "TaskMind"
    };
    public string DisplayInitial =>
        string.IsNullOrWhiteSpace(DisplayName) ? "T" : DisplayName.Trim()[0].ToString().ToUpperInvariant();

    private async Task LoginAsync()
    {
        if (IsCredentialEntryStep &&
            (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password)))
        {
            StatusMessage = "Vui lòng nhập email và mật khẩu.";
            return;
        }
        if (IsRoleSelectionStep && SelectedRole == "Tự nhận diện")
        {
            StatusMessage = "Hãy chọn vai trò tài khoản để tiếp tục.";
            return;
        }

        await RunBusyAsync(async () =>
        {
            var role = IsRoleSelectionStep ? RoleOptions.IndexOf(SelectedRole) : (int?)null;
            var body = new LoginRequest(Email.Trim(), Password, role);
            LoginResult result;
            try
            {
                result = await _api.PostAsync<LoginRequest, LoginResult>(
                    ApiBaseUrl, "auth/login", body);
            }
            catch (HttpRequestException exception) when (
                IsCredentialEntryStep &&
                exception.Message.Contains(
                    "Email này liên kết với nhiều tài khoản.",
                    StringComparison.Ordinal))
            {
                SelectedRole = RoleOptions[0];
                IsAccountSelectionRequired = true;
                StatusMessage = "Email và mật khẩu hợp lệ. Chọn vai trò tài khoản để tiếp tục.";
                return;
            }
            if (string.IsNullOrWhiteSpace(result.AccessToken))
                throw new InvalidOperationException("API đăng nhập không trả về access token.");

            _accessToken = result.AccessToken;
            _api.SetAccessToken(_accessToken);
            DisplayName = string.IsNullOrWhiteSpace(result.FullName) ? result.Email : result.FullName;
            AccountRole = result.Role;
            ProfileEmail = result.Email;
            ProfileName = DisplayName;
            IsAccountSelectionRequired = false;
            SelectedRole = RoleOptions[0];
            IsAuthenticated = true;
            Password = string.Empty;
            SelectedSection = "Overview";
            StatusMessage = $"Đã đăng nhập với vai trò {result.Role}.";
            await LoadDashboardAsync();
        });
    }

    private async Task RefreshCurrentSectionAsync()
    {
        if (!IsAuthenticated || IsBusy)
            return;
        await RunBusyAsync(async () =>
        {
            switch (SelectedSection)
            {
                case "Overview":
                    await LoadDashboardAsync();
                    break;
                case "Projects":
                    await LoadProjectsDataAsync();
                    StatusMessage = $"Đã tải {Projects.Count} dự án.";
                    break;
                case "Skills":
                    await LoadSkillsDataAsync();
                    StatusMessage = $"Đã tải {Skills.Count} kỹ năng.";
                    break;
                case "Profile":
                    await LoadProfileAsync();
                    break;
            }
        });
    }

    private async Task LoadDashboardAsync()
    {
        await LoadProjectsDataAsync();
        await LoadSkillsDataAsync();
        StatusMessage = "Dữ liệu tổng quan đã được cập nhật.";
    }

    private async Task LoadProjectsAsync() =>
        await RunBusyAsync(async () =>
        {
            await LoadProjectsDataAsync();
            StatusMessage = $"Đã tải {Projects.Count} dự án.";
        });

    private async Task LoadSkillsAsync() =>
        await RunBusyAsync(async () =>
        {
            await LoadSkillsDataAsync();
            StatusMessage = $"Đã tải {Skills.Count} kỹ năng.";
        });

    private async Task LoadProjectsDataAsync()
    {
        var query = string.IsNullOrWhiteSpace(ProjectSearch)
            ? "projects?page=1&pageSize=8"
            : $"projects?keyword={Uri.EscapeDataString(ProjectSearch.Trim())}&page=1&pageSize=20";
        var page = await _api.GetAsync<ProjectPage>(ApiBaseUrl, query);
        ReplaceItems(Projects, page.Items);
        ProjectCount = page.TotalCount;
    }

    private async Task LoadSkillsDataAsync()
    {
        var query = string.IsNullOrWhiteSpace(SkillSearch)
            ? "skills?page=1&pageSize=8"
            : $"skills?keyword={Uri.EscapeDataString(SkillSearch.Trim())}&page=1&pageSize=50";
        var page = await _api.GetAsync<SkillPage>(ApiBaseUrl, query);
        ReplaceItems(Skills, page.Items);
        SkillCount = page.TotalCount;
    }

    private async Task LoadProfileAsync()
    {
        var account = await _api.GetAsync<CurrentAccount>(ApiBaseUrl, "me");
        ProfileEmail = account.Email;
        ProfileName = account.FullName;
        ProfileBio = string.IsNullOrWhiteSpace(account.Bio)
            ? "Chưa có nội dung giới thiệu."
            : account.Bio;
        DisplayName = string.IsNullOrWhiteSpace(account.FullName) ? account.Email : account.FullName;
        AccountRole = account.Role;
        StatusMessage = "Hồ sơ đã được cập nhật.";
    }

    private async Task RunBusyAsync(Func<Task> action)
    {
        if (IsBusy)
            return;
        IsBusy = true;
        try
        {
            await action();
        }
        catch (HttpRequestException exception)
        {
            StatusMessage = exception.Message;
        }
        catch (TaskCanceledException)
        {
            StatusMessage = "Kết nối API đã hết thời gian chờ. Hãy kiểm tra API và thử lại.";
        }
        catch (ArgumentException exception)
        {
            StatusMessage = exception.Message;
        }
        catch (InvalidOperationException exception)
        {
            StatusMessage = exception.Message;
        }
        catch (JsonException exception)
        {
            StatusMessage = $"Không đọc được dữ liệu API: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void Logout()
    {
        _accessToken = null;
        _api.SetAccessToken(null);
        IsAuthenticated = false;
        AccountRole = string.Empty;
        DisplayName = "Khách";
        Projects.Clear();
        Skills.Clear();
        ProjectCount = 0;
        SkillCount = 0;
        StatusMessage = "Đã đăng xuất khỏi TaskMind.";
    }

    private static void ReplaceItems<T>(ObservableCollection<T> target, IEnumerable<T> items)
    {
        target.Clear();
        foreach (var item in items)
            target.Add(item);
    }

    public void Dispose() => _api.Dispose();

    public sealed class ProjectItem
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string SourceType { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public int ActiveMemberCount { get; init; }
    }

    public sealed class SkillItem
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
    }

    private sealed record LoginRequest(string Email, string Password, int? Role);
    private sealed class LoginResult
    {
        public string Email { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string Role { get; init; } = string.Empty;
        public string AccessToken { get; init; } = string.Empty;
    }
    private sealed class ProjectPage
    {
        public IReadOnlyCollection<ProjectItem> Items { get; init; } = Array.Empty<ProjectItem>();
        public int TotalCount { get; init; }
    }
    private sealed class SkillPage
    {
        public IReadOnlyCollection<SkillItem> Items { get; init; } = Array.Empty<SkillItem>();
        public int TotalCount { get; init; }
    }
    private sealed class CurrentAccount
    {
        public string Email { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string Role { get; init; } = string.Empty;
        public string Bio { get; init; } = string.Empty;
    }
}
