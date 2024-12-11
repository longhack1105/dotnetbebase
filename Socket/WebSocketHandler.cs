using ChatApp.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Services.Users;
using System.Net.WebSockets;
using System.Text;
using TWChatAppApiMaster.Databases.ChatApp;
using TWChatAppApiMaster.Models.DataInfo;

namespace ChatApp.Socket
{
    public abstract class WebSocketHandler
    {
        protected ConnectionManager ConnectionManager { get; set; }

        public WebSocketHandler(ConnectionManager connectionManager)
        {
            ConnectionManager = connectionManager;
        }

        public abstract Session validateSession(string sessionUuid);
        public abstract Task handleOtherDeviceLogin(string token);

        public abstract void processMessage(string user, string message);

        public virtual async Task OnConnected(WebSocket socket, string sessionUuid)
        {
            var session = validateSession(sessionUuid);

            if (session != null)
            {
                bool isSuccess = ConnectionManager.AddSocket(session, socket);

                if (isSuccess)
                {
                    var _context = ServiceExtension.GetDbContext();
                    try
                    {
                        session.IsOnline = 1;
                        session.TimeConnectSocket = DateTime.UtcNow;

                        _context.Session.Update(session);

                        var acc = _context.Account
                            .FirstOrDefault(x => x.UserName == session.UserName);

                        if (acc != null)
                        {
                            acc.LastSeen = DateTime.Now;
                            _context.Account.Update(acc);
                        }

                        await _context.SaveChangesAsync();
                    }
                    finally
                    {
                        _context.Dispose();
                    }
                }
            }
            else
            {
                await ConnectionManager.RemoveSocket(socket, "Token Invalid ...");
            }
        }

        public virtual async Task OnDisconnected(WebSocket socket)
        {
            var uuid = ConnectionManager.GetUuidBySocket(socket);
            if (!string.IsNullOrEmpty(uuid))
            {
                var _context = ServiceExtension.GetDbContext();
                try
                {
                    var session = await _context.Session.OrderByDescending(x => x.Id).FirstOrDefaultAsync(x => x.Uuid == uuid);
                    if (session != null)
                    {
                        session.TimeDisconnectSocket = DateTime.UtcNow;
                        session.IsOnline = 0;

                        var acc = _context.Account
                            .FirstOrDefault(x => x.UserName == session.UserName);

                        if (acc != null)
                        {
                            acc.LastSeen = DateTime.Now;
                            _context.Account.Update(acc);
                        }

                        await _context.SaveChangesAsync();
                    }
                }
                finally
                {
                    _context.Dispose();
                }
            }    

            await ConnectionManager.RemoveSocket(socket);
        }

        public async Task SendMessageAsync(WebSocket socket, string message)
        {
            if (socket.State != WebSocketState.Open)
                return;

            await socket.SendAsync(buffer: new ArraySegment<byte>(array: Encoding.ASCII.GetBytes(message),
                                                                  offset: 0,
                                                                  count: message.Length),
                                   messageType: WebSocketMessageType.Text,
                                   endOfMessage: true,
                                   cancellationToken: CancellationToken.None);
        }

        public async Task SendMessageAsync(string user, string message)
        {
            if (!CheckUserIsOnline(user)) return;

            var sockets = ConnectionManager.GetSocketByUser(user);
            foreach ( var socketDto in sockets)
            {
                await SendMessageAsync(socketDto.WebSocket, message);
            }
        }

        public async Task SendMessageToAllAsync(string message)
        {
            foreach (var socketDto in ConnectionManager.GetAllWebSockets())
            {
                if (socketDto.WebSocket.State == WebSocketState.Open)
                    await SendMessageAsync(socketDto.WebSocket, message);
            }
        }

        public async Task SendMessageToGroupUsersAsync(string message, List<string> lstUsers)
        {
            foreach (var user in lstUsers)
            {
                await SendMessageAsync(user, message);
            }
        }

        public async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

            await SendMessageToAllAsync(message);
        }

        public string ReceiveString(WebSocketReceiveResult result, byte[] buffer)
        {
            return Encoding.UTF8.GetString(buffer, 0, result.Count);
        }

        public async Task BroadcastMessage(string message)
        {
            await SendMessageToAllAsync(message);
        }

        public List<string> GetAllUsers()
        {
            return ConnectionManager.GetAllUsers();
        }

        public bool CheckUserIsOnline(string user)
        {
            return ConnectionManager.GetAllUsers().Contains(user);
        }

        public string GetUserBySocket(WebSocket socket)
        {
            return ConnectionManager.GetUserBySocket(socket);
        }

        public List<SocketDTO> GetSocketByUser(string user)
        {
            return ConnectionManager.GetSocketByUser(user);
        }
    }
}
