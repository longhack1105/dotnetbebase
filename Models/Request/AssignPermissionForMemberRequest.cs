using ChatApp.Models.Request;

namespace TWChatAppApiMaster.Models.Request
{
    public class AssignPermissionForMemberRequest : UuidRequest
    {
        public string UserUuid { get; set; }
        public bool? ChangeGroupInfo { get; set; }
        public bool? DeleteMessage { get; set; }
        public bool? BanUser { get; set; }
        public bool? AddMember { get; set; }
        public bool? LockMember { get; set; }
    }
}
