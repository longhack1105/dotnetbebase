using ChatApp.Models.Request;

namespace ChatApp.Models.Request
{
    public class GetListAccountRequest : KeywordRequest
    {
        public sbyte? RoleId { get; set; }
        public sbyte? LockState { get; set; }
    }
}
