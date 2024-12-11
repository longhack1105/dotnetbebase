using ChatApp.Models.Request;

namespace TWChatAppApiMaster.Models.Request
{
    public class ChangeMakeFriendStateRequest : UuidRequest
    {
        public String MemberUuid { get; set; }
        public int State { get; set; }
    }
}
