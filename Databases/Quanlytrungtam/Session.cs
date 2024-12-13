using System;
using System.Collections.Generic;

namespace DotnetBeBase.Databases.Quanlytrungtam
{
    public partial class Session
    {
        public int Id { get; set; }
        public string Uuid { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Token { get; set; } = null!;
        public DateTime TimeExpired { get; set; }
        public string RefreshToken { get; set; } = null!;
        public DateTime TimeRefreshExpired { get; set; }
        public string Username { get; set; } = null!;
        public string AccountUuid { get; set; } = null!;
        /// <summary>
        /// 1 - active; 2 - destroy
        /// </summary>
        public sbyte State { get; set; }

        public virtual Account UsernameNavigation { get; set; } = null!;
    }
}
