using System;
using System.Collections.Generic;

namespace ChatApp.Models.DataInfo
{
    public class FindMemberDTO
    {
        public string Uuid { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Avatar { get; set; } = null!;
        public int? RoleId { get; set; }
        public DateTime LastSeen { get; set; }

    }
}
