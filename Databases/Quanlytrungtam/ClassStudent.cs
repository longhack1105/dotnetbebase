using System;
using System.Collections.Generic;

namespace DotnetBeBase.Databases.Quanlytrungtam
{
    public partial class ClassStudent
    {
        public int Id { get; set; }
        public string ClassUuid { get; set; } = null!;
        public string StudentUuid { get; set; } = null!;

        public virtual Class ClassUu { get; set; } = null!;
        public virtual Student StudentUu { get; set; } = null!;
    }
}
