using System.Net.WebSockets;

namespace DotnetBeBase.Models.Dtos
{
    public class SocketDTO
    {
        public string Uuid {  get; set; } = string.Empty;
        public WebSocket WebSocket { get; set; }
    }
}
