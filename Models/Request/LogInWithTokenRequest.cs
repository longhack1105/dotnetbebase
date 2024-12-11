namespace ChatApp.Models.Request
{
    public class LogInWithTokenRequest : UserBaseRequest
    {
        //public string Password { get; set; }
        public string? Ip { get; set; }
        public string? Address { get; set; }
        public string Os { get; set; }
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string? FcmToken { get; set; }
        public string AccessToken { get; set; } = string.Empty;
    }
}
