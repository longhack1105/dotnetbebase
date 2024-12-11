using System;
using System.Collections.Generic;

namespace TWChatAppApiMaster.Databases.ChatApp
{
    /// <summary>
    /// Ghim cuộc trò chuyện
    /// </summary>
    public partial class RoomPin
    {
        public int Id { get; set; }
        public string RoomUuid { get; set; } = null!;
        public string UserName { get; set; } = null!;
        /// <summary>
        /// 0: unpin  1:pin
        /// </summary>
        public sbyte? State { get; set; }
        public DateTime TimePin { get; set; }

        public virtual Rooms RoomUu { get; set; } = null!;
        public virtual Account UserNameNavigation { get; set; } = null!;
    }
}
