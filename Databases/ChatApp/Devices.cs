using System;
using System.Collections.Generic;

namespace TWChatAppApiMaster.Databases.ChatApp
{
    public partial class Devices
    {
        public long Id { get; set; }
        public string UserName { get; set; } = null!;
        public string DeviceId { get; set; } = null!;
        public string Os { get; set; } = null!;
        public string DeviceName { get; set; } = null!;
        public string? Address { get; set; }
        public string? Ip { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime? LastUsed { get; set; }
        /// <summary>
        /// 1: Using - 2: Inacctive
        /// </summary>
        public sbyte Status { get; set; }

        public virtual Account UserNameNavigation { get; set; } = null!;
    }
}
