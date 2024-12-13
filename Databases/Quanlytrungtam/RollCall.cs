using System;
using System.Collections.Generic;

namespace DotnetBeBase.Databases.Quanlytrungtam
{
    public partial class RollCall
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string StudentUuid { get; set; } = null!;
        public string ClassUuid { get; set; } = null!;
        public string RoomUuid { get; set; } = null!;
        public DateTime TimeSheet { get; set; }
    }
}
