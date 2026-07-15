namespace TaskMind.WPFs.Modules.Auths.Models
{
    public enum ForgotPasswordStep
    {
        EnterEmail,
        EnterOtp,
        ResetPassword,
        Done
    }

    public class ForgotPasswordModel
    {
        public string Email { get; set; }
        public string OtpCode { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}