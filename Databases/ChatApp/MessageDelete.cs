using System;
using System.Collections.Generic;

namespace TWChatAppApiMaster.Databases.ChatApp
{
    public partial class MessageDelete
    {
        public long Id { get; set; }
        public string MessageUuid { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public DateTime TimeCreated { get; set; }

        public virtual Messages MessageUu { get; set; } = null!;
    }
}
