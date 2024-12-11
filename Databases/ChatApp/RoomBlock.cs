using System;
using System.Collections.Generic;

namespace TWChatAppApiMaster.Databases.ChatApp
{
    /// <summary>
    /// Chặn người dùng trong nhóm
    /// </summary>
    public partial class RoomBlock
    {
        public long Id { get; set; }
        public string RoomUuid { get; set; } = null!;
        public string UserName { get; set; } = null!;
        /// <summary>
        /// 0 = unblocked; 
        /// 1 = blocked; 
        /// 
        /// </summary>
        public sbyte State { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime? TimeUpdated { get; set; }

        public virtual Rooms RoomUu { get; set; } = null!;
        public virtual Account UserNameNavigation { get; set; } = null!;
    }
}
