using System.ComponentModel.DataAnnotations;
using ChatApp.Models.Request;

namespace ChatApp.Models.Request
{
    public class UpsertGroupMemberRequest : DpsParamBase
    {
        public string GroupUuid{ get; set; }
        //public string NewMemberUuid { get; set;}
        public List<string> ListNewMemberUuid { get; set;}
        public sbyte RoleId { get; set;}
        public sbyte Type { get; set; } //1: add 0: remove
    }
}
