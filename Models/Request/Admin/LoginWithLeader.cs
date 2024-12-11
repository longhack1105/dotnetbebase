namespace TWChatAppApiMaster.Models.Request.Admin
{
    public class LoginWithLeader
    {
        public string UserName {  get; set; } = string.Empty;
        public string? Ip { get; set; }
        public string? Address { get; set; }
        public string? Os { get; set; }
        public string? DeviceId { get; set; }
        public string? DeviceName { get; set; }
        public string? FcmToken { get; set; }
    }
}
