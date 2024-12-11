using ChatApp.Models.Request;

namespace TWChatAppApiMaster.Models.Request
{
    public class ChangeAvatarAccountRequest : DpsParamBase
    {
        public string AvatarPath { get; set; }
    }
}
