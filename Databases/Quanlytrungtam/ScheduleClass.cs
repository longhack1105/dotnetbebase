using System;
using System.Collections.Generic;

namespace DotnetBeBase.Databases.Quanlytrungtam
{
    public partial class ScheduleClass
    {
        public int Id { get; set; }
        public string ScheduleUuid { get; set; } = null!;
        public string ClassUuid { get; set; } = null!;

        public virtual Class ClassUu { get; set; } = null!;
        public virtual Schedule ScheduleUu { get; set; } = null!;
    }
}
