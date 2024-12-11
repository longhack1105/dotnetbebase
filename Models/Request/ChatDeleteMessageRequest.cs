using ChatApp.Models.Request;

namespace TWChatAppApiMaster.Models.Request
{
    public class ChatDeleteMessageRequest : UuidRequest
    {
        /// <summary>
        /// 1: Xoá với mọi người
        /// 2: Xoá với mình tôi
        /// </summary>
        public sbyte Type { get; set; } = 1;
    }

    public class ChatDeleteMessagesRequest : UuidListRequest
    {
        /// <summary>
        /// 1: Xoá với mọi người
        /// 2: Xoá với mình tôi
        /// </summary>
        public sbyte Type { get; set; } = 1;
        public string? UserName { get; set; }
        public string RoomUuid { get; set; } = string.Empty;
    }
}
