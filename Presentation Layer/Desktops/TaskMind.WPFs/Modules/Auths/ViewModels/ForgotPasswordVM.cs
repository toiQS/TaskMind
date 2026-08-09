using System.Text.RegularExpressions;
using System.Windows.Input;
using MediatR;
using TaskMind.WPFs.Modules.Auths.Models;
using TaskMind.WPFs.Utilities;
using AuthSendOtpCommand = TaskMind.Applications.Auths.Features.SendOtpCommand;
using AuthVerifyOtpCommand = TaskMind.Applications.Auths.Features.VerifyOtpCommand;
using AuthResetPasswordCommand = TaskMind.Applications.Auths.Features.ResetPasswordCommand;

namespace TaskMind.WPFs.Modules.Auths.ViewModels
{
    public class ForgotPasswordVM : ViewModelBase
    {
        private readonly IMediator _mediator;

        private ForgotPasswordStep _currentStep = ForgotPasswordStep.EnterEmail;
        public ForgotPasswordStep CurrentStep { get => _currentStep; set { _currentStep = value; OnPropertyChanged(); } }

        private string _email;
        public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }

        private string _otpCode;
        public string OtpCode { get => _otpCode; set { _otpCode = value; OnPropertyChanged(); } }

        private string _newPassword;
        public string NewPassword { get => _newPassword; set { _newPassword = value; OnPropertyChanged(); } }

        private string _confirmPassword;
        public string ConfirmPassword { get => _confirmPassword; set { _confirmPassword = value; OnPropertyChanged(); } }

        private string _errorMessage;
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); } }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

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

        public ForgotPasswordVM() : this(null) { }

        public ForgotPasswordVM(IMediator mediator)
        {
            _mediator = MediatorResolver.Resolve(mediator);

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

        private async Task SendOtpAsync(bool isResend = false)
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Email) || !Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ErrorMessage = "Email không hợp lệ.";
                return;
            }

            IsBusy = true;
            try
            {
                await _mediator.Send(new AuthSendOtpCommand { Email = Email.Trim() });

                if (!isResend)
                    CurrentStep = ForgotPasswordStep.EnterOtp;

                _ = StartResendCooldownAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
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

        private async Task VerifyOtpAsync()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(OtpCode) || OtpCode.Length != 6)
            {
                ErrorMessage = "Vui lòng nhập đủ 6 số mã xác thực.";
                return;
            }

            IsBusy = true;
            try
            {
                await _mediator.Send(new AuthVerifyOtpCommand { Email = Email.Trim(), OtpCode = OtpCode.Trim() });
                CurrentStep = ForgotPasswordStep.ResetPassword;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

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
            try
            {
                await _mediator.Send(new AuthResetPasswordCommand
                {
                    Email = Email.Trim(),
                    OtpCode = OtpCode.Trim(),
                    NewPassword = NewPassword,
                    ConfirmPassword = ConfirmPassword
                });

                CurrentStep = ForgotPasswordStep.Done;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}