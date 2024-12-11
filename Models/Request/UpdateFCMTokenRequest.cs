using ChatApp.Models.Request;

namespace ChatApp.Models.Request
{
    public class UpdateFCMTokenRequest : DpsParamBase
    {
        public string? Token { get; set; }
        public string? Uuid { get; set; }
    }
}
