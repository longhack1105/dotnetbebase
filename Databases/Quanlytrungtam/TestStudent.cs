using System;
using System.Collections.Generic;

namespace DotnetBeBase.Databases.Quanlytrungtam
{
    public partial class TestStudent
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string StudentUuid { get; set; } = null!;
        public decimal? Grade { get; set; }
        public string? Note { get; set; }

        public virtual Student StudentUu { get; set; } = null!;
    }
}
