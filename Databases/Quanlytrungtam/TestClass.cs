using System;
using System.Collections.Generic;

namespace DotnetBeBase.Databases.Quanlytrungtam
{
    public partial class TestClass
    {
        public int Id { get; set; }
        public string TestUuid { get; set; } = null!;
        public string ClassUuid { get; set; } = null!;
        public int ScheduleScheduleSheetRoomId { get; set; }

        public virtual Class ClassUu { get; set; } = null!;
        public virtual ScheduleScheduleSheetRoom ScheduleScheduleSheetRoom { get; set; } = null!;
        public virtual Test TestUu { get; set; } = null!;
    }
}
