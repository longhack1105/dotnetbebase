using System;
using System.Collections.Generic;

namespace TWChatAppApiMaster.Databases.ChatApp
{
    /// <summary>
    /// Ghim tin nhắn
    /// </summary>
    public partial class MessagePin
    {
        public long Id { get; set; }
        public string RoomUuid { get; set; } = null!;
        public string MessageUuid { get; set; } = null!;
        public string UserName { get; set; } = null!;
        /// <summary>
        /// 0: unpin 1: pin
        /// </summary>
        public sbyte State { get; set; }
        public DateTime? TimePin { get; set; }

        public virtual Messages MessageUu { get; set; } = null!;
        public virtual Rooms RoomUu { get; set; } = null!;
        public virtual Account UserNameNavigation { get; set; } = null!;
    }
}
