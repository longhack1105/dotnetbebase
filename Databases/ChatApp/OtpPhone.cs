using System;
using System.Collections.Generic;

namespace TWChatAppApiMaster.Databases.ChatApp
{
    public partial class OtpPhone
    {
        public long Id { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string Otp { get; set; } = null!;
        public DateTime TimeCreated { get; set; }
        public DateTime? TimeExpired { get; set; }
        public string? UserUsed { get; set; }
        public string? Note { get; set; }
        /// <summary>
        /// 1: Active, 0: Off
        /// </summary>
        public sbyte Status { get; set; }
        /// <summary>
        /// 1: Register, 2: Fogot password, 3: Change password
        /// </summary>
        public sbyte Action { get; set; }
    }
}
