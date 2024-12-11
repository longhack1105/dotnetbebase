using System;
using System.Collections.Generic;

namespace TWChatAppApiMaster.Databases.ChatApp
{
    public partial class RoomMembers
    {
        public long Id { get; set; }
        public string RoomUuid { get; set; } = null!;
        public string UserName { get; set; } = null!;
        /// <summary>
        /// 1 = Trưởng nhóm; 
        /// 2 = Phó nhóm; 
        /// 3 = Thành viên thường; 
        /// </summary>
        public sbyte RoleId { get; set; }
        /// <summary>
        /// 0 = leaved; 
        /// 1 = inroom;  
        /// </summary>
        public ulong InRoom { get; set; }
        /// <summary>
        /// 0 = Không cho phép kết bạn; 
        /// 1 = Được phép kết bạn;
        /// </summary>
        public ulong CanMakeFriend { get; set; }
        /// <summary>
        /// chức năng thay đổi thông tin nhóm
        /// </summary>
        public ulong ChangeGroupInfo { get; set; }
        /// <summary>
        /// chức năng xoá tin nhắn
        /// </summary>
        public ulong DeleteMessage { get; set; }
        /// <summary>
        /// chức năng ban thành viên
        /// </summary>
        public ulong BanUser { get; set; }
        /// <summary>
        /// chức năng thêm thành viên
        /// </summary>
        public ulong AddMember { get; set; }
        /// <summary>
        /// chức năng khoá/mở thành viên
        /// </summary>
        public ulong LockMember { get; set; }
        /// <summary>
        /// chức năng chặn/mở chặn thành viên
        /// </summary>
        public ulong BlockMember { get; set; }

        public virtual Rooms RoomUu { get; set; } = null!;
        public virtual Account UserNameNavigation { get; set; } = null!;
    }
}
