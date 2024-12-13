using System;
using System.Collections.Generic;

namespace DotnetBeBase.Databases.Quanlytrungtam
{
    public partial class ScheduleScheduleSheetRoom
    {
        public ScheduleScheduleSheetRoom()
        {
            TestClass = new HashSet<TestClass>();
        }

        public int Id { get; set; }
        public string ScheduleUuid { get; set; } = null!;
        public string ScheduleSheetUuid { get; set; } = null!;
        public string RoomUuid { get; set; } = null!;

        public virtual Room RoomUu { get; set; } = null!;
        public virtual ScheduleSheet ScheduleSheetUu { get; set; } = null!;
        public virtual Schedule ScheduleUu { get; set; } = null!;
        public virtual ICollection<TestClass> TestClass { get; set; }
    }
}
