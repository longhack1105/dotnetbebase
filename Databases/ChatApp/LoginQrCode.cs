using System;
using System.Collections.Generic;

namespace TWChatAppApiMaster.Databases.ChatApp
{
    public partial class LoginQrCode
    {
        public long Id { get; set; }
        /// <summary>
        /// key tạo QR code login (bằng với uuid session)
        /// </summary>
        public string Uuid { get; set; } = null!;
        public DateTime TimeExpired { get; set; }
        public DateTime TimeCreated { get; set; }
        public string DeviceId { get; set; } = null!;
        public string Os { get; set; } = null!;
        public string DeviceName { get; set; } = null!;
        public string? Address { get; set; }
        public string? Ip { get; set; }
        public string? FcmToken { get; set; }
    }
}
