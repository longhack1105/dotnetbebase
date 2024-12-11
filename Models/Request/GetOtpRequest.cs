namespace ChatApp.Models.Request
{
    public class GetOtpRequest : DpsParamBase
    {
        public string? Otp { get; set; }
        public string PhoneNumber { get; set; }
        public int Action { get; set; }
    }
}
