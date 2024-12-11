using System;
using System.Collections.Generic;

namespace TWChatAppApiMaster.Databases.ChatApp
{
    /// <summary>
    /// Đăng ký tự động xoá tin nhắn
    /// </summary>
    public partial class RegisterAutoDelete
    {
        public long Id { get; set; }
        public string UserName { get; set; } = null!;
        public string RoomUuid { get; set; } = null!;
        public int? PeriodTime { get; set; }
        public DateTime? LastTimeDelete { get; set; }

        public virtual Rooms RoomUu { get; set; } = null!;
        public virtual Account UserNameNavigation { get; set; } = null!;
    }
}
