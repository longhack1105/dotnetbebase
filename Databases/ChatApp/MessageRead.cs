using System;
using System.Collections.Generic;

namespace TWChatAppApiMaster.Databases.ChatApp
{
    /// <summary>
    /// Tin nhắn đã đọc
    /// </summary>
    public partial class MessageRead
    {
        public long Id { get; set; }
        public string RoomUuid { get; set; } = null!;
        /// <summary>
        /// Tin nhắn đọc cuối cùng
        /// </summary>
        public long LastMessageId { get; set; }
        public string UserName { get; set; } = null!;
        public DateTime TimeRead { get; set; }

        public virtual Messages LastMessage { get; set; } = null!;
        public virtual Rooms RoomUu { get; set; } = null!;
        public virtual Account UserNameNavigation { get; set; } = null!;
    }
}
