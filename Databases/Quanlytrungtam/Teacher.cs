using System;
using System.Collections.Generic;

namespace DotnetBeBase.Databases.Quanlytrungtam
{
    public partial class Teacher
    {
        public Teacher()
        {
            ClassTeacher = new HashSet<ClassTeacher>();
        }

        public int Id { get; set; }
        public string Uuid { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public long? Salary { get; set; }
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Email { get; set; } = null!;

        public virtual ICollection<ClassTeacher> ClassTeacher { get; set; }
    }
}
