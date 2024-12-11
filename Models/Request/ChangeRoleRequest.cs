using ChatApp.Models.Request;

namespace ChatApp.Models.Request
{
    public class ChangeRoleRequest : DpsParamBase
    {
        public sbyte? RoleId { get; set; }
        public String? Uuid { get; set; }
    }
}
