using System;
using System.Collections.Generic;

namespace DotnetBeBase.Databases.Quanlytrungtam
{
    public partial class Equipment
    {
        public int Id { get; set; }
        public string Uuid { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Name { get; set; } = null!;
        public string? Des { get; set; }
        public long? Price { get; set; }
        public sbyte Type { get; set; }
        public string? RoomUuid { get; set; }

        public virtual Room? RoomUu { get; set; }
        public virtual EquipmentType TypeNavigation { get; set; } = null!;
    }
}
