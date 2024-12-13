using System;
using System.Collections.Generic;

namespace DotnetBeBase.Databases.Quanlytrungtam
{
    public partial class Assessment
    {
        public int Id { get; set; }
        public string Uuid { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string StudentUuid { get; set; } = null!;
        public string? ClassUuid { get; set; }
        public string? RoomUuid { get; set; }
        public DateTime? TimeSheet { get; set; }
        public string Name { get; set; } = null!;
        public string? Note { get; set; }
        /// <summary>
        /// 0: đánh giá chung - 1: khen - 2: chê
        /// </summary>
        public sbyte Type { get; set; }

        public virtual Class? ClassUu { get; set; }
        public virtual Student StudentUu { get; set; } = null!;
    }
}
