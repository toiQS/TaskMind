using System.Text.RegularExpressions;
using System.Windows.Input;
using TaskMind.WPFs.Modules.Auths.Models;
using TaskMind.WPFs.Utilities;

namespace TaskMind.WPFs.Modules.Auths.ViewModels
{
    public class ForgotPasswordVM : ViewModelBase
    {
        private ForgotPasswordStep _currentStep = ForgotPasswordStep.EnterEmail;
        public ForgotPasswordStep CurrentStep
        {
            get => _currentStep;
            set { _currentStep = value; OnPropertyChanged(); }
        }

        private string _email;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        private string _otpCode;
        public string OtpCode
        {
            get => _otpCode;
            set { _otpCode = value; OnPropertyChanged(); }
        }

        private string _newPassword;
        public string NewPassword
        {
            get => _newPassword;
            set { _newPassword = value; OnPropertyChanged(); }
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { _confirmPassword = value; OnPropertyChanged(); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        private int _resendCooldownSeconds;
        public int ResendCooldownSeconds
        {
            get => _resendCooldownSeconds;
            set { _resendCooldownSeconds = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanResend)); }
        }

        public bool CanResend => ResendCooldownSeconds <= 0;

        public ICommand SendOtpCommand { get; }
        public ICommand ResendOtpCommand { get; }
        public ICommand VerifyOtpCommand { get; }
        public ICommand ResetPasswordCommand { get; }
        public ICommand BackCommand { get; }

        public ForgotPasswordVM()
        {
            SendOtpCommand = new RelayCommand(async _ => await SendOtpAsync());
            ResendOtpCommand = new RelayCommand(async _ => await SendOtpAsync(isResend: true));
            VerifyOtpCommand = new RelayCommand(async _ => await VerifyOtpAsync());
            ResetPasswordCommand = new RelayCommand(async _ => await ResetPasswordAsync());
            BackCommand = new RelayCommand(_ => GoBack());
        }

        private void GoBack()
        {
            ErrorMessage = string.Empty;
            CurrentStep = CurrentStep switch
            {
                ForgotPasswordStep.EnterOtp => ForgotPasswordStep.EnterEmail,
                ForgotPasswordStep.ResetPassword => ForgotPasswordStep.EnterOtp,
                _ => ForgotPasswordStep.EnterEmail
            };
        }

        /// <summary>TODO: gọi service POST /auth/forgot-password gửi OTP về email thật.</summary>
        private async Task SendOtpAsync(bool isResend = false)
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Email) || !Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ErrorMessage = "Email không hợp lệ.";
                return;
            }

            IsBusy = true;
            await Task.Delay(500);
            IsBusy = false;

            if (!isResend)
                CurrentStep = ForgotPasswordStep.EnterOtp;

            _ = StartResendCooldownAsync();
        }

        private async Task StartResendCooldownAsync()
        {
            ResendCooldownSeconds = 60;
            while (ResendCooldownSeconds > 0)
            {
                await Task.Delay(1000);
                ResendCooldownSeconds--;
            }
        }

        /// <summary>TODO: gọi service POST /auth/verify-otp xác thực mã thật.</summary>
        private async Task VerifyOtpAsync()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(OtpCode) || OtpCode.Length != 6)
            {
                ErrorMessage = "Vui lòng nhập đủ 6 số mã xác thực.";
                return;
            }

            IsBusy = true;
            await Task.Delay(500);
            IsBusy = false;

            // TODO: nếu OTP sai, service trả lỗi -> gán ErrorMessage và return, không chuyển bước.
            CurrentStep = ForgotPasswordStep.ResetPassword;
        }

        /// <summary>TODO: gọi service POST /auth/reset-password đặt mật khẩu mới thật.</summary>
        private async Task ResetPasswordAsync()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(NewPassword) || NewPassword.Length < 8)
            {
                ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự.";
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                ErrorMessage = "Mật khẩu xác nhận không khớp.";
                return;
            }

            IsBusy = true;
            await Task.Delay(500);
            IsBusy = false;

            CurrentStep = ForgotPasswordStep.Done;
        }
    }
}