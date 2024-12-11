using System;
using System.Collections.Generic;

namespace TWChatAppApiMaster.Databases.ChatApp
{
    public partial class RoomRecent
    {
        public long Id { get; set; }
        public string RoomUuid { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public DateTime TimeCreated { get; set; }
        public DateTime? TimeUpdated { get; set; }
        public int? Count { get; set; }

        public virtual Rooms RoomUu { get; set; } = null!;
        public virtual Account UserNameNavigation { get; set; } = null!;
    }
}
