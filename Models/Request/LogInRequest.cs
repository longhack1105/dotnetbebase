namespace ChatApp.Models.Request
{
    public class LogInRequest : UserBaseRequest
    {
        public string Password { get; set; }
        public string? Ip { get; set; }
        public string? Address { get; set; }
        public string Os { get; set; }
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string? FcmToken { get; set; }
    }
}
