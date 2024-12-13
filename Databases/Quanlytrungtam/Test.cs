using System;
using System.Collections.Generic;

namespace DotnetBeBase.Databases.Quanlytrungtam
{
    public partial class Test
    {
        public Test()
        {
            TestClass = new HashSet<TestClass>();
        }

        public int Id { get; set; }
        public string Uuid { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Name { get; set; } = null!;
        public string Des { get; set; } = null!;
        public sbyte Type { get; set; }

        public virtual TestType TypeNavigation { get; set; } = null!;
        public virtual ICollection<TestClass> TestClass { get; set; }
    }
}
