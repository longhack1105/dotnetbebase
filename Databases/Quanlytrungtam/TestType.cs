using System;
using System.Collections.Generic;

namespace DotnetBeBase.Databases.Quanlytrungtam
{
    public partial class TestType
    {
        public TestType()
        {
            Test = new HashSet<Test>();
        }

        public sbyte Id { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<Test> Test { get; set; }
    }
}
