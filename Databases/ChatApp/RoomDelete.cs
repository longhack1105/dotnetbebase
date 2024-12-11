using System;
using System.Collections.Generic;

namespace TWChatAppApiMaster.Databases.ChatApp
{
    public partial class RoomDelete
    {
        public long Id { get; set; }
        public string RoomUuid { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public long? LastMessageId { get; set; }
        public DateTime TimeCreated { get; set; }

        public virtual Messages? LastMessage { get; set; }
        public virtual Rooms RoomUu { get; set; } = null!;
        public virtual Account UserNameNavigation { get; set; } = null!;
    }
}
