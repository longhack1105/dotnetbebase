using System;
using System.Collections.Generic;

namespace DotnetBeBase.Databases.Quanlytrungtam
{
    public partial class Room
    {
        public Room()
        {
            Equipment = new HashSet<Equipment>();
            ScheduleScheduleSheetRoom = new HashSet<ScheduleScheduleSheetRoom>();
        }

        public int Id { get; set; }
        public string Uuid { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Name { get; set; } = null!;
        public sbyte Status { get; set; }

        public virtual RoomStatus StatusNavigation { get; set; } = null!;
        public virtual ICollection<Equipment> Equipment { get; set; }
        public virtual ICollection<ScheduleScheduleSheetRoom> ScheduleScheduleSheetRoom { get; set; }
    }
}
