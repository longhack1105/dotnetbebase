using System;
using System.Collections.Generic;

namespace TWChatAppApiMaster.Databases.ChatApp
{
    public partial class Notifications
    {
        public long Id { get; set; }
        public string Uuid { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public sbyte Type { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public int ActionId { get; set; }
        public DateTime TimeCreated { get; set; }
        /// <summary>
        /// 0: Chưa xem - 1: Đã xem
        /// </summary>
        public sbyte Status { get; set; }

        public virtual Account UserNameNavigation { get; set; } = null!;
    }
}
