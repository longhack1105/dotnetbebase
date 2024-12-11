using ChatApp.Models.Request;

namespace TWChatAppApiMaster.Models.Request
{
    public class ChangeGroupInfoRequest : UuidRequest
    {
        public string GroupName { get; set; }
        public string GroupAvatar { get; set; }
    }
}
