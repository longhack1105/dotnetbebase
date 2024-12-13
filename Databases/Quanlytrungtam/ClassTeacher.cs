using System;
using System.Collections.Generic;

namespace DotnetBeBase.Databases.Quanlytrungtam
{
    public partial class ClassTeacher
    {
        public int Id { get; set; }
        public string ClassUuid { get; set; } = null!;
        public string TeacherUuid { get; set; } = null!;

        public virtual Class ClassUu { get; set; } = null!;
        public virtual Teacher TeacherUu { get; set; } = null!;
    }
}
