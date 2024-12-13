using System;
using System.Collections.Generic;

namespace DotnetBeBase.Databases.Quanlytrungtam
{
    public partial class Schedule
    {
        public Schedule()
        {
            ScheduleClass = new HashSet<ScheduleClass>();
            ScheduleScheduleSheetRoom = new HashSet<ScheduleScheduleSheetRoom>();
        }

        public int Id { get; set; }
        public string Uuid { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Name { get; set; } = null!;
        public DateTime? TimeStart { get; set; }
        public DateTime? TimeEnd { get; set; }
        /// <summary>
        /// 0: Hoạt động, 1: Ngừng hoạt động
        /// </summary>
        public sbyte Status { get; set; }

        public virtual ICollection<ScheduleClass> ScheduleClass { get; set; }
        public virtual ICollection<ScheduleScheduleSheetRoom> ScheduleScheduleSheetRoom { get; set; }
    }
}
