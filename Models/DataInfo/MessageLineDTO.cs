using TWChatAppApiMaster.Databases.ChatApp;
using System;
using System.Collections.Generic;
using TWChatAppApiMaster.Models.DataInfo;

namespace ChatApp.Models.DataInfo
{
    public class MessageLineDTO
    {
        public string Uuid { get; set; } = null!;
        public string MsgRoomUuid { get; set; } = null!;
        public string RoomName { get; set; } = null!;
        public string RoomAvatar { get; set; } = null!;
        public string? ReplyMsgUuid { get; set; }
        public string UserSent { get; set; } = null!;
        public string UserSentUuid { get; set; } = null!;
        public bool IsBan { get; set; } = false;
        public bool IsBlock { get; set; } = false;
        public bool IsDeleteWithMe { get; set; } = false;
        public string FullName { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string OwnerUuid { get; set; } = null!;
        public string CountryCode { get; set; } = null!;
        public string Avatar { get; set; } = null!;
        /// <summary>
        /// 1: Text - 2: Link - 3: Image - 4: Video - 5: Audio -  6: Void Call - 7: Video Call
        /// </summary>
        public sbyte ContentType { get; set; }
        public DateTime TimeForward { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime LastEdited { get; set; }

        /// <summary>
        /// 1: Normal; 2: editted; 3: hide with member; 4: deleted;
        /// </summary>
        public sbyte Status { get; set; }
        /// <summary>
        /// 1: PP - 2: Group
        /// </summary>
        public sbyte Type { get; set; }
        public int LikeCount { get; set; }
        public sbyte ReadState { get; set; }
        public string ForwardFrom { get; set; }
        public string MediaName { get; set; }
        public string? FileInfo { get; set; }
        public List<ReactedDTO> EmojiList { get; set; } = new();
        public MessageLineDTO? ReplyMsgUu { get; set; }
    }
}
