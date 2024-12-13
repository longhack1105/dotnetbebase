using System;
using System.Collections.Generic;

namespace DotnetBeBase.Databases.Quanlytrungtam
{
    public partial class Account
    {
        public Account()
        {
            Session = new HashSet<Session>();
        }

        public int Id { get; set; }
        public string Uuid { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        /// <summary>
        /// 0: Pending - 1: Active - 2:Deactive
        /// </summary>
        public sbyte State { get; set; }
        public string? FullName { get; set; }
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        /// <summary>
        /// 0: admin - 1: giáo viên - 2: học sinh
        /// </summary>
        public sbyte Role { get; set; }
        public string? Avatar { get; set; }
        /// <summary>
        /// 0:email - 1:phone
        /// </summary>
        public sbyte RegisterType { get; set; }
        public string? RoleUuid { get; set; }

        public virtual ICollection<Session> Session { get; set; }
    }
}
