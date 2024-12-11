using System;
using System.Collections.Generic;

namespace TWChatAppApiMaster.Databases.ChatApp
{
    public partial class Session
    {
        public long Id { get; set; }
        public string Uuid { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime LoginTime { get; set; }
        public string? FcmToken { get; set; }
        public DateTime TimeExpired { get; set; }
        public DateTime TimeExpiredRefresh { get; set; }
        /// <summary>
        /// 0: Logging - 1: LogOut
        /// </summary>
        public sbyte Status { get; set; }
        public DateTime? LogoutTime { get; set; }
        public string? DeviceId { get; set; }
        public string? Ip { get; set; }
        public ulong IsOnline { get; set; }
        public DateTime? TimeConnectSocket { get; set; }
        public DateTime? TimeDisconnectSocket { get; set; }

        public virtual Account UserNameNavigation { get; set; } = null!;
    }
}
