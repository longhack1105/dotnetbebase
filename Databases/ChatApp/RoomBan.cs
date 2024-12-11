using System;
using System.Collections.Generic;

namespace TWChatAppApiMaster.Databases.ChatApp
{
    /// <summary>
    /// Cấm chat trong nhóm
    /// </summary>
    public partial class RoomBan
    {
        public long Id { get; set; }
        public string RoomUuid { get; set; } = null!;
        public string UserName { get; set; } = null!;
        /// <summary>
        /// 0 = unbaned; 
        /// 1 = banned; 
        /// 
        /// </summary>
        public sbyte State { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime? TimeUpdated { get; set; }

        public virtual Rooms RoomUu { get; set; } = null!;
        public virtual Account UserNameNavigation { get; set; } = null!;
    }
}
