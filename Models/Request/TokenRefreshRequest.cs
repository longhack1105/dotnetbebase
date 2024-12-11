using ChatApp.Models.Request;
using System.ComponentModel.DataAnnotations;

namespace TWChatAppApiMaster.Models.Request
{
    public class TokenRefreshRequest : DpsParamBase
    {
        [Required]
        public string RefreshToken { get; set; }
        public string? FcmToken { get; set; }
    }
}
