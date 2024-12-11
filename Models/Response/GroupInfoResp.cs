
using System.Text.Json.Serialization;

namespace ChatApp.Models.Response
{
    public class GroupInfoResp
    {
        public int MemCount { get; set; }
        public int AutoDelete { get; set; }
        public List<RoomMembersDto> RoomMembers { get; set; }
        public PermissionDto Permissions { get; set; } = new PermissionDto();
        public class RoomMembersDto { 
            public string Uuid { get; set; }
            public string UserName { get; set; }
            public string FullName { get; set; }
            public string Avatar { get; set; }
            public DateTime? TimeCreated { get; set; }
            public int? Status { get; set; }
            public int? RoleId { get; set; }
            public int? RoomRoleId { get; set; }
            public bool IsFriend { get; set; }
            public bool IsOnline { get; set; }
            public int? CanMakeFriend { get; set; }
        }
        public class PermissionDto
        {
            public ulong ChangeGroupInfo { get; set; }
            public ulong DeleteMessage { get; set; }
            public ulong BanUser { get; set; }
            public ulong AddMember { get; set; }
            public ulong LockMember { get; set; }
        }
    }
}
