using ChatApp.Models.DataInfo;
using TWChatAppApiMaster.Models.DataInfo;

namespace TWChatAppApiMaster.Socket
{
    public class Message
    {
        public int MsgType { get; set; }
        public string Data { get; set; }
    }
}
