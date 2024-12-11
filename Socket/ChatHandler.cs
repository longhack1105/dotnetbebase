using ChatApp.Extensions;
using ChatApp.Models.DataInfo;
using ChatApp.Queue;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Services.Aad;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using System.Text.Json;
using TWChatAppApiMaster.Databases.ChatApp;
using TWChatAppApiMaster.Socket;
using static ChatApp.Enums.EnumDatabase;
using Microsoft.AspNetCore.Mvc;
using static Google.Apis.Requests.BatchRequest;
using static TWChatAppApiMaster.Models.Response.Admin.GroupGetListResp;
using Microsoft.VisualStudio.Services.Account;
using Microsoft.VisualStudio.Services.Users;
using static Microsoft.VisualStudio.Services.Graph.GraphResourceIds;
using FirebaseAdmin.Messaging;
using System.Security.Principal;

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
                var session = _context.Session.AsNoTracking().OrderByDescending(x => x.Id).FirstOrDefault(x => x.Uuid == sessionUuid && x.Status == 0 && x.TimeExpiredRefresh > DateTime.UtcNow);
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

        private TWChatAppApiMaster.Socket.Message TryDeserializeMessage(string str)
        {
            try
            {
                using (JsonDocument doc = JsonDocument.Parse(str))
                {
                    JsonElement root = doc.RootElement;
                    var message = new TWChatAppApiMaster.Socket.Message();
                    message.MsgType = root.GetProperty("MsgType").GetInt32();

                    JsonElement dataElement = root.GetProperty("Data");
                    message.Data = dataElement.ToString();

                    return message;
                }

                //return System.Text.Json.JsonSerializer.Deserialize<TWChatAppApiMaster.Socket.Message>(str);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public override async Task<object> handleOtherDeviceLogin(string userName)
        {
            var responseData = new TWChatAppApiMaster.Socket.Message
            {
                MsgType = (int)MessageType.TYPE_OTHER_DEVICE_LOGIN,
                Data = ""
            };
            await SendMessageAsync(userName, JsonConvert.SerializeObject(responseData));
            return null;
        }
    }
}
