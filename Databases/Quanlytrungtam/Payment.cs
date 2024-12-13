using System;
using System.Collections.Generic;

namespace DotnetBeBase.Databases.Quanlytrungtam
{
    public partial class Payment
    {
        public int Id { get; set; }
        public string Uuid { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string StudentUuid { get; set; } = null!;
        public long Amount { get; set; }
        public sbyte Status { get; set; }

        public virtual PaymentStatus StatusNavigation { get; set; } = null!;
        public virtual Student StudentUu { get; set; } = null!;
    }
}
