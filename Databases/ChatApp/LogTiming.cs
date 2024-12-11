using System;
using System.Collections.Generic;

namespace TWChatAppApiMaster.Databases.ChatApp
{
    public partial class LogTiming
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public int TimeHandle { get; set; }
        public string Request { get; set; } = null!;
        public string Response { get; set; } = null!;
        public DateTime TimeCreated { get; set; }
    }
}
