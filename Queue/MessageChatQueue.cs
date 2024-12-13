using DotnetBeBase.Databases.Quanlytrungtam;
using ChatApp.Socket;
using System;
using System.Collections.Generic;

namespace ChatApp.Queue
{
    public class MessageChatQueue
    {
        public string Uuid { get; set; } = null!;
        public string MsgRoomUuid { get; set; } = null!;
        public string? ReplyMsgUuid { get; set; }
        public string UserSent { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string CountryCode { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Avatar { get; set; } = null!;
        public string RoomAvatar { get; set; } = null!;
        public string FileInfo { get; set; } = null!;
        /// <summary>
        /// 1: Text - 2: Link - 3: Image - 4: Video - 5: Audio
        /// </summary>
        public sbyte ContentType { get; set; }
        public List<string>? ListUsersToSendNotify { get; set; }
        public List<string>? ListUsersOnline { get; set; }
        public List<string>? ListUsersOffline { get; set; }
    }
}
