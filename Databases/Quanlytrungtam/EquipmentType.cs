using System;
using System.Collections.Generic;

namespace DotnetBeBase.Databases.Quanlytrungtam
{
    public partial class EquipmentType
    {
        public EquipmentType()
        {
            Equipment = new HashSet<Equipment>();
        }

        public sbyte Id { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<Equipment> Equipment { get; set; }
    }
}
