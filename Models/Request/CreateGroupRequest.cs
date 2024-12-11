using System.ComponentModel.DataAnnotations;
using ChatApp.Models.Request;

namespace ChatApp.Models.Request
{
    public class CreateGroupRequest : DpsParamBase
    {
        public string GroupName{ get; set; }
        public string GroupAvatar { get; set; }
        public List<string>? MemberUuids { get; set;}
    }
}
