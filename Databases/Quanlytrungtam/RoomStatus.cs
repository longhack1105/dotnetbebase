using System;
using System.Collections.Generic;

namespace DotnetBeBase.Databases.Quanlytrungtam
{
    public partial class RoomStatus
    {
        public RoomStatus()
        {
            Room = new HashSet<Room>();
        }

        public sbyte Id { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<Room> Room { get; set; }
    }
}
