using System;
using System.Collections.Generic;

namespace ChatApp.Models.DataInfo
{
    public class GroupDetailDTO
    {
        public string Uuid { get; set; } = null!;
        public string GroupName { get; set; } = null!;
        public string? Avatar { get; set; }
        public int NumberOfMember {  get; set; }
        public DateTime TimeCreated { get; set; }
        public sbyte Status { get; set; }
    }
}
