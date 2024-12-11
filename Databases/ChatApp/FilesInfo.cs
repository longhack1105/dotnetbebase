using System;
using System.Collections.Generic;

namespace TWChatAppApiMaster.Databases.ChatApp
{
    public partial class FilesInfo
    {
        public long Id { get; set; }
        public string Uuid { get; set; } = null!;
        public string UserUpload { get; set; } = null!;
        public DateTime TimeCreated { get; set; }
        public string? FileName { get; set; }
        public string? Path { get; set; }
    }
}
