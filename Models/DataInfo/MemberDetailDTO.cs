using System;
using System.Collections.Generic;

namespace ChatApp.Models.DataInfo
{
    public class MemberDetailDTO
    {
        public string? Uuid { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? FullName { get; set; }
        public string? Avatar { get; set; }
        public DateTime? TimeCreated { get; set; }
        public sbyte? Status { get; set; }
        public int RoleId { get; set; }
        public bool IsFriend { get; set; }
        public sbyte? FriendRequestStatus { get; set; }
        public sbyte? FriendRequestSendStatus { get; set; }
        public bool IsOnline { get; set; }
        public int CanMakeFriend { get; set; }
        public bool IsBan { get; set; }
        public bool IsBlock { get; set; }
    }
}
