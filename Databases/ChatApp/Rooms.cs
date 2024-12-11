using System;
using System.Collections.Generic;

namespace TWChatAppApiMaster.Databases.ChatApp
{
    public partial class Rooms
    {
        public Rooms()
        {
            MessagePin = new HashSet<MessagePin>();
            MessageRead = new HashSet<MessageRead>();
            Messages = new HashSet<Messages>();
            RegisterAutoDelete = new HashSet<RegisterAutoDelete>();
            RoomBan = new HashSet<RoomBan>();
            RoomBlock = new HashSet<RoomBlock>();
            RoomDelete = new HashSet<RoomDelete>();
            RoomMembers = new HashSet<RoomMembers>();
            RoomPin = new HashSet<RoomPin>();
            RoomRecent = new HashSet<RoomRecent>();
        }

        public long Id { get; set; }
        public string Uuid { get; set; } = null!;
        /// <summary>
        /// 1: PP - 2: Group
        /// </summary>
        public sbyte Type { get; set; }
        public string? LastMessageUuid { get; set; }
        /// <summary>
        /// 1: Normal - 2: Pin - 3: Delete Only me - 4: Delete All - 5: Revoke
        /// </summary>
        public sbyte Status { get; set; }
        public string? RoomName { get; set; }
        public string Creater { get; set; } = null!;
        public DateTime TimeCreated { get; set; }
        public DateTime LastUpdated { get; set; }
        /// <summary>
        /// Được phép chat với type = 1
        /// </summary>
        public ulong IsAllow { get; set; }
        /// <summary>
        /// Với type = 2
        /// </summary>
        public string? Avatar { get; set; }

        public virtual Account CreaterNavigation { get; set; } = null!;
        public virtual Messages? LastMessageUu { get; set; }
        public virtual ICollection<MessagePin> MessagePin { get; set; }
        public virtual ICollection<MessageRead> MessageRead { get; set; }
        public virtual ICollection<Messages> Messages { get; set; }
        public virtual ICollection<RegisterAutoDelete> RegisterAutoDelete { get; set; }
        public virtual ICollection<RoomBan> RoomBan { get; set; }
        public virtual ICollection<RoomBlock> RoomBlock { get; set; }
        public virtual ICollection<RoomDelete> RoomDelete { get; set; }
        public virtual ICollection<RoomMembers> RoomMembers { get; set; }
        public virtual ICollection<RoomPin> RoomPin { get; set; }
        public virtual ICollection<RoomRecent> RoomRecent { get; set; }
    }
}
