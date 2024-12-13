using System;
using System.Collections.Generic;

namespace DotnetBeBase.Databases.Quanlytrungtam
{
    public partial class ScheduleSheet
    {
        public ScheduleSheet()
        {
            ScheduleScheduleSheetRoom = new HashSet<ScheduleScheduleSheetRoom>();
        }

        public int Id { get; set; }
        public string Uuid { get; set; } = null!;
        /// <summary>
        /// format: HH:mm
        /// </summary>
        public string TimeStart { get; set; } = null!;
        /// <summary>
        /// format: HH:mm
        /// </summary>
        public string TimeEnd { get; set; } = null!;
        /// <summary>
        /// 2 -&gt; 8
        /// </summary>
        public sbyte DayInWeek { get; set; }

        public virtual ICollection<ScheduleScheduleSheetRoom> ScheduleScheduleSheetRoom { get; set; }
    }
}
