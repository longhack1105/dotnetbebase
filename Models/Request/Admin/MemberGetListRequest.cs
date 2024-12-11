using ChatApp.Models.Request;

namespace TWChatAppApiMaster.Models.Request.Admin
{
    public class MemberGetListRequest : KeywordRequest
    {
        public bool? IsOnline { get; set; }
        public sbyte? RoleId { get; set; }
    }
}
