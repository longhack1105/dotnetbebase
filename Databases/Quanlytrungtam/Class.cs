using System;
using System.Collections.Generic;

namespace DotnetBeBase.Databases.Quanlytrungtam
{
    public partial class Class
    {
        public Class()
        {
            Assessment = new HashSet<Assessment>();
            ClassStudent = new HashSet<ClassStudent>();
            ClassTeacher = new HashSet<ClassTeacher>();
            ScheduleClass = new HashSet<ScheduleClass>();
            TestClass = new HashSet<TestClass>();
        }

        public int Id { get; set; }
        public string Uuid { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Name { get; set; } = null!;
        public string? Des { get; set; }
        public long Price { get; set; }

        public virtual ICollection<Assessment> Assessment { get; set; }
        public virtual ICollection<ClassStudent> ClassStudent { get; set; }
        public virtual ICollection<ClassTeacher> ClassTeacher { get; set; }
        public virtual ICollection<ScheduleClass> ScheduleClass { get; set; }
        public virtual ICollection<TestClass> TestClass { get; set; }
    }
}
