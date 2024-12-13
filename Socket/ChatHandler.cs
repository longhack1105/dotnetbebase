using ChatApp.Extensions;
using ChatApp.Queue;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Services.Aad;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using System.Text.Json;
using DotnetBeBase.Socket;
using static ChatApp.Enums.EnumDatabase;
using Microsoft.AspNetCore.Mvc;
using static Google.Apis.Requests.BatchRequest;
using Microsoft.VisualStudio.Services.Account;
using Microsoft.VisualStudio.Services.Users;
using static Microsoft.VisualStudio.Services.Graph.GraphResourceIds;
using FirebaseAdmin.Messaging;
using System.Security.Principal;
using DotnetBeBase.Databases.Quanlytrungtam;

namespace ChatApp.Socket
{
    public class ChatHandler : WebSocketHandler
    {
        //private readonly ILogger<ChatHandler> _logger;

        //public ChatHandler(ILogger<ChatHandler> logger)
        //{
        //    _logger = logger;
        //}

        public static ChatHandler handleInstance;
        public static ChatHandler getInstance()
        {
            return handleInstance;
        }
        public enum MessageType
        {
            TYPE_OTHER_DEVICE_LOGIN = 0,
        }

        public ChatHandler(ConnectionManager connectionManager) : base(connectionManager)
        {

        }

        public override Session validateSession(string sessionUuid)
        {
            var _context = ServiceExtension.GetDbContext();

            try
            {
                var session = _context.Session.AsNoTracking().OrderByDescending(x => x.Id).FirstOrDefault(x => x.Uuid == sessionUuid && x.State == 1 && x.TimeExpired > DateTime.UtcNow);
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

        /// <summary>
        /// Điều hướng đến các hàm sử lý websocket
        /// </summary>
        /// <param name="user"></param>
        /// <param name="message"></param>
        public override void processMessage(string user, string message)
        {
            var _context = ServiceExtension.GetDbContext();

            try
            {
                var msg = TryDeserializeMessage(message);
                if (msg != null)
                {
                    
                }
            }
            catch (Exception error)
            {

                throw;
            }
            finally
            {
                _context.Dispose();
            }
        }

        private DotnetBeBase.Socket.Message TryDeserializeMessage(string str)
        {
            try
            {
                using (JsonDocument doc = JsonDocument.Parse(str))
                {
                    JsonElement root = doc.RootElement;
                    var message = new DotnetBeBase.Socket.Message();
                    message.MsgType = root.GetProperty("MsgType").GetInt32();

                    JsonElement dataElement = root.GetProperty("Data");
                    message.Data = dataElement.ToString();

                    return message;
                }

                //return System.Text.Json.JsonSerializer.Deserialize<DotnetBeBase.Socket.Message>(str);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public override async Task<object> handleOtherDeviceLogin(string userName)
        {
            var responseData = new DotnetBeBase.Socket.Message
            {
                MsgType = (int)MessageType.TYPE_OTHER_DEVICE_LOGIN,
                Data = ""
            };
            await SendMessageAsync(userName, JsonConvert.SerializeObject(responseData));
            return null;
        }
    }
}
