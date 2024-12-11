using System.Net.WebSockets;

namespace TWChatAppApiMaster.Models.DataInfo
{
    public class SocketDTO
    {
        public string Uuid {  get; set; } = string.Empty;
        public WebSocket WebSocket { get; set; }
    }
}
