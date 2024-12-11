using System;
using System.Collections.Generic;

namespace TWChatAppApiMaster.Databases.ChatApp
{
    public partial class Messages
    {
        public Messages()
        {
            InverseReplyMessageUu = new HashSet<Messages>();
            MessageDelete = new HashSet<MessageDelete>();
            MessageLike = new HashSet<MessageLike>();
            MessagePin = new HashSet<MessagePin>();
            MessageRead = new HashSet<MessageRead>();
            RoomDelete = new HashSet<RoomDelete>();
            Rooms = new HashSet<Rooms>();
        }

        public long Id { get; set; }
        public string Uuid { get; set; } = null!;
        public string UserSent { get; set; } = null!;
        public string RoomUuid { get; set; } = null!;
        public string? ReplyMessageUuid { get; set; }
        public string Content { get; set; } = null!;
        /// <summary>
        /// 1: Text - 2: Link - 3: Image - 4: Video - 5: Audio - 6: Void Call - 7: Video Call
        /// </summary>
        public sbyte ContentType { get; set; }
        /// <summary>
        /// 1: Normal; 
        /// 2: editted; 
        /// 3: hide with member; 
        /// 4: deleted;
        /// </summary>
        public sbyte Status { get; set; }
        public string LanguageCode { get; set; } = null!;
        public string? ForwardFrom { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime LastEdited { get; set; }
        /// <summary>
        /// ảnh thumb video do client gửi
        /// </summary>
        public string? FileInfo { get; set; }

        public virtual Messages? ReplyMessageUu { get; set; }
        public virtual Rooms RoomUu { get; set; } = null!;
        public virtual Account UserSentNavigation { get; set; } = null!;
        public virtual ICollection<Messages> InverseReplyMessageUu { get; set; }
        public virtual ICollection<MessageDelete> MessageDelete { get; set; }
        public virtual ICollection<MessageLike> MessageLike { get; set; }
        public virtual ICollection<MessagePin> MessagePin { get; set; }
        public virtual ICollection<MessageRead> MessageRead { get; set; }
        public virtual ICollection<RoomDelete> RoomDelete { get; set; }
        public virtual ICollection<Rooms> Rooms { get; set; }
    }
}
