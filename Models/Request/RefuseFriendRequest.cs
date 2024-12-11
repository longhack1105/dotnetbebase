using ChatApp.Models.Request;

namespace TWChatAppApiMaster.Models.Request
{
    public class RefuseFriendRequest : UuidRequest
    {
        public int Type { get; set; }
    }
}
