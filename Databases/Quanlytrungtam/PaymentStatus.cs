using System;
using System.Collections.Generic;

namespace DotnetBeBase.Databases.Quanlytrungtam
{
    public partial class PaymentStatus
    {
        public PaymentStatus()
        {
            Payment = new HashSet<Payment>();
        }

        public sbyte Id { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<Payment> Payment { get; set; }
    }
}
