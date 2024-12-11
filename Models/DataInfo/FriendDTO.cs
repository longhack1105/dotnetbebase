using System;
using System.Collections.Generic;

namespace ChatApp.Models.DataInfo
{
    public class FriendDTO
    {
        public string Uuid { get; set; } = null!;
        public string FriendUserName { get; set; } = null!;
        public string FriendFullName { get; set; } = null!;
        /// <summary>
        /// 1: Friend - 2: Blocked
        /// </summary>
        public sbyte Type { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime LastUpdated { get; set; }
        /// <summary>
        /// 0: Đề nghị - 1: Chấp nhận - 2: Từ chối
        /// </summary>
        public sbyte Status { get; set; }
        public int CanMakeFriend { get; set; }
        public string Avatar { get; set; } = null!;
        public DateTime? LastSeen { get; set; }
    }
}
