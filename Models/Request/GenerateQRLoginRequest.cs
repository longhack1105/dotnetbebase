using ChatApp.Models.Request;

namespace TWChatAppApiMaster.Models.Request
{
    public class GenerateQRLoginRequest : DpsParamBase
    {
        public string? Ip { get; set; }
        public string? Address { get; set; }
        public string Os { get; set; }
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string? FcmToken { get; set; }
    }
}
