using System;
using System.Collections.Generic;

namespace DotnetBeBase.Databases.Quanlytrungtam
{
    public partial class Student
    {
        public Student()
        {
            Assessment = new HashSet<Assessment>();
            ClassStudent = new HashSet<ClassStudent>();
            Payment = new HashSet<Payment>();
            TestStudent = new HashSet<TestStudent>();
        }

        public int Id { get; set; }
        public string Uuid { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string FullName { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Zalo { get; set; }
        public string? Messenge { get; set; }

        public virtual ICollection<Assessment> Assessment { get; set; }
        public virtual ICollection<ClassStudent> ClassStudent { get; set; }
        public virtual ICollection<Payment> Payment { get; set; }
        public virtual ICollection<TestStudent> TestStudent { get; set; }
    }
}
