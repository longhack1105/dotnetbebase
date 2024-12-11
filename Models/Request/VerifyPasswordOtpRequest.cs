namespace ChatApp.Models.Request
{
    public class VerifyPasswordOtpRequest : DpsParamBase
    {
        public string Email { get; set; }
        public string Otp { get; set; }
    }
}
