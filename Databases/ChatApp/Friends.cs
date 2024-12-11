using System;
using System.Collections.Generic;

namespace TWChatAppApiMaster.Databases.ChatApp
{
    /// <summary>
    /// Bạn bè
    /// </summary>
    public partial class Friends
    {
        public int Id { get; set; }
        public string Uuid { get; set; } = null!;
        /// <summary>
        /// Người gửi lời mời
        /// </summary>
        public string UserSent { get; set; } = null!;
        /// <summary>
        /// Người nhận lời mời
        /// </summary>
        public string UserReceiver { get; set; } = null!;
        /// <summary>
        /// 1: Friend - 2: Blocked
        /// </summary>
        public sbyte Type { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime LastUpdated { get; set; }
        /// <summary>
        /// 1: ko bạn bè; 
        /// 2: Chờ xác nhận; 
        /// 3: Bạn bè; 
        /// </summary>
        public sbyte Status { get; set; }

        public virtual Account UserReceiverNavigation { get; set; } = null!;
        public virtual Account UserSentNavigation { get; set; } = null!;
    }
}
