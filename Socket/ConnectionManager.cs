using System.Collections.Concurrent;
using System.Net.WebSockets;
using TWChatAppApiMaster.Databases.ChatApp;
using TWChatAppApiMaster.Models.DataInfo;

namespace ChatApp.Socket
{
    public class ConnectionManager
    {
        private static ConcurrentDictionary<string, List<SocketDTO>> _sockets = new ConcurrentDictionary<string, List<SocketDTO>>();

        public ConcurrentDictionary<string, List<SocketDTO>> GetAllSockets()
        {
            return _sockets;
        }

        public List<SocketDTO> GetAllWebSockets()
        {
            return _sockets.SelectMany(x => x.Value).ToList();
        }

        public List<string> GetAllUsers()
        {
            return _sockets.Select(p => p.Key).ToList();
        }

        public List<SocketDTO> GetSocketByUser(string user)
        {
            return _sockets.Where(p => p.Key == user).LastOrDefault().Value;
        }

        public string GetUserBySocket(WebSocket socket)
        {
            return _sockets.LastOrDefault(p => p.Value.Select(x => x.WebSocket).Contains(socket)).Key;
        }

        public string? GetUuidBySocket(WebSocket socket)
        {
            return _sockets.SelectMany(x => x.Value).LastOrDefault(x => x.WebSocket == socket)?.Uuid;
        }

        public bool AddSocket(Session session, WebSocket socket)
        {
            bool result = false;
            if (_sockets.Any(x => x.Key == session.UserName))
            {
                var userSockets = _sockets.Last(x => x.Key == session.UserName);

                if (!userSockets.Value.Select(x => x.WebSocket).Contains(socket))
                {
                    userSockets.Value.Add(new SocketDTO
                    {
                        Uuid = session.Uuid,
                        WebSocket = socket,
                    });

                    result = true;
                }    
            }    
            else
            {
                _sockets.TryAdd(session.UserName, new List<SocketDTO>() { new SocketDTO
                {
                    Uuid = session.Uuid,
                    WebSocket = socket,
                }});

                result = true;
            }

            return result;
        }

        public async Task RemoveSocket(WebSocket socket, string description = "Connection closed")
        {
            if (socket != null)
            {
                try
                {
                    if (_sockets.Any(p => p.Value.Select(x => x.WebSocket).Contains(socket)))
                    {
                        var userSockets = _sockets.Last(p => p.Value.Select(x => x.WebSocket).Contains(socket));

                        if (userSockets.Value.Count() > 1)
                        {
                            var socketDto = userSockets.Value.Where(x => x.WebSocket == socket).First();
                            userSockets.Value.Remove(socketDto);
                        }
                        else
                        {
                            _sockets.TryRemove(userSockets);
                        }
                    }

                    if (socket.State != WebSocketState.Aborted && socket.State != WebSocketState.Closed)
                    {
                        await socket.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure,
                                            statusDescription: description,
                                            cancellationToken: CancellationToken.None);
                    }
                }
                catch (Exception)
                {

                }

            }
        }

        public async void RemoveAllSocket()
        {
            var socketDicList = _sockets.ToList();
            foreach (var socket in socketDicList.SelectMany(x => x.Value))
            {
                if (socket.WebSocket.State != WebSocketState.Aborted && socket.WebSocket.State != WebSocketState.Closed)
                {
                    await socket.WebSocket.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure,
                                        statusDescription: "Connection closed",
                                        cancellationToken: CancellationToken.None);
                }
            }

            foreach (var socketDic in socketDicList)
            {
                _sockets.TryRemove(socketDic);
            }
        }

        public bool UserAlreadyExists(string user)
        {
            return _sockets.ContainsKey(user);
        }
    }
}
