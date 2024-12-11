using ChatApp.Extensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Net.WebSockets;
using TWChatAppApiMaster.Databases.ChatApp;

namespace ChatApp.Socket
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private WebSocketHandler _webSocketHandler { get; set; }

        public WebSocketMiddleware(RequestDelegate next, WebSocketHandler webSocketHandler)
        {
            _next = next;
            _webSocketHandler = webSocketHandler;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _next.Invoke(context);
                return;
            }

            string sessionUuid = context.Request.Query["session"];
            //var session = validateToken(token);

            WebSocket socket = await context.WebSockets.AcceptWebSocketAsync();
            await _webSocketHandler.OnConnected(socket, sessionUuid);

            await Receive(socket, async (result, buffer) =>
            {
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var msg = _webSocketHandler.ReceiveString(result, buffer);

                    await HandleMessage(socket, msg);

                    return;
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await HandleDisconnect(socket);
                    return;
                }
            });
        }

        private async Task HandleDisconnect(WebSocket socket)
        {
            await _webSocketHandler.OnDisconnected(socket);
        }

        private async Task HandleMessage(WebSocket socket, string message)
        {
            if (message == "ping")
            {
                await _webSocketHandler.SendMessageAsync(user: _webSocketHandler.GetUserBySocket(socket), "pong");
            }
            else
            {
                _webSocketHandler.processMessage(_webSocketHandler.GetUserBySocket(socket), message);
            }
        }

        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];

            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer),
                                                           cancellationToken: CancellationToken.None);

                    handleMessage(result, buffer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                await HandleDisconnect(socket);
            }
        }

        private async Task<Session> validateToken(string token)
        {
            var _context = ServiceExtension.GetDbContext();

            try
            {
                var session = _context.Session.AsNoTracking().OrderByDescending(x => x.Id).FirstOrDefault(x => x.AccessToken == token && x.Status == 0 && x.TimeExpired > DateTime.UtcNow);
                if (session != null)
                {
                    return session;
                }
            }
            finally
            {
                _context.Dispose();
            }

            return null;
        }
    }
}
