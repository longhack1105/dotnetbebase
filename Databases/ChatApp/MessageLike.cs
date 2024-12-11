using System;
using System.Collections.Generic;

namespace TWChatAppApiMaster.Databases.ChatApp
{
    /// <summary>
    /// Like tin nhắn
    /// </summary>
    public partial class MessageLike
    {
        public long Id { get; set; }
        public string MessageUuid { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public DateTime TimeCreated { get; set; }
        /// <summary>
        /// 1: Tim - 2: Like - 3: Cười ...
        /// </summary>
        public sbyte Type { get; set; }
        /// <summary>
        /// 1: Enable - 0: Disable
        /// </summary>
        public sbyte Status { get; set; }

        public virtual Messages MessageUu { get; set; } = null!;
        public virtual Account UserNameNavigation { get; set; } = null!;
    }
}
