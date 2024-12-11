using System;
using System.Collections.Generic;

namespace ChatApp.Models.DataInfo
{
    public class MessagesGroupDTO
    {
        public long? Id { get; set; }
        public string Uuid { get; set; } = null!;
        public string UserSent { get; set; } = null!;
        public string FullName { get; set; } = null!;
        /// <summary>
        /// 1: PP - 2: Group
        /// </summary>
        public sbyte Type { get; set; }
        public string CreatorFullName { get; set; }
        public string OwnerUuid { get; set; } = null!;
        public string PartnerUuid { get; set; } = null!;
        public string? ShowName { get; set; } = null!;
        public string? ShowUuid { get; set; } = null!;
        public string LastMsgLineUuid { get; set; } = null!;
        public string Content { get; set; } = null!;
        /// <summary>
        /// 1: Text - 2: Link - 3: Image - 4: Video - 5: Audio -  6: Void Call - 7: Video Call
        /// </summary>
        public sbyte ContentType { get; set; }
        public bool IsDeleteWithMe { get; set; } = false;
        public int ReadCounter { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime LastUpdated { get; set; }
        /// <summary>
        /// 1: Normal; 2: editted; 3: hide with member; 4: deleted;
        /// </summary>
        public sbyte Status { get; set; }

        public string OwnerName { get; set; }
        public int LikeCount { get; set; }
        public int UnreadCount { get; set; }
        //public int UnreadTotal { get; set; }
        public string Avatar { get; set; }
        public bool Pinned { get; set; }
        public string ForwardFrom { get; set; }
        /// <summary>
        /// 0: unblock - 1: block
        /// </summary>
        public sbyte UserSentIsBlock { get; set; }
        public string? FileInfo { get; set; } = string.Empty;
    }
}
