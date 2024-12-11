using ChatApp.Extensions;
using ChatApp.Firebase;
using ChatApp.Queue;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using TWChatAppApiMaster.Databases.ChatApp;
using static ChatApp.Socket.ChatHandler;
using Task = System.Threading.Tasks.Task;

namespace ChatApp.Timers
{
    public class TimerProcessMessageDb : IHostedService, IDisposable
    {
        private readonly ILogger<TimerProcessMessageDb> _logger;
        private Timer _timer;
        private int TimeLoop = 2 * 1000; // ms

        public TimerProcessMessageDb(ILogger<TimerProcessMessageDb> logger)
        {
            _logger = logger;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        /// <summary>
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            ChangeStateTimer(true);
            _logger.LogInformation("Processor message running.");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processor message is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private async void ChangeStateTimer(bool enable)
        {
            _timer ??= new Timer(DoWork, null, Timeout.Infinite, Timeout.Infinite);

            var time_wait = enable ? TimeLoop : Timeout.Infinite;
            _timer.Change(0, time_wait);
        }

        private void DoWork(object state)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            var _context = ServiceExtension.GetDbContext();

            try
            {
                while (true)
                {
                    var msgQueue = MessageQueueManager.dequeue();
                    
                    if (msgQueue != null)
                    {
                        HandleMessenge(msgQueue);
                        ////send socket to user sent
                        //var data = new TWChatAppApiMaster.Socket.Message
                        //{
                        //    MsgType = (int)MessageType.TYPE_CHAT,
                        //    Data = JsonConvert.SerializeObject(msgQueue.ServerMsg)
                        //};
                        //SendMessageAsync(msgQueue.UserSent, JsonConvert.SerializeObject(data));

                        //if (msgQueue.Content.Length > 2040)
                        //{
                        //    msgQueue.Content = msgQueue.Content.Substring(0, 2040);
                        //}

                        //if (msgQueue.ListUsersToSendNotify != null && msgQueue.ListUsersToSendNotify.Count > 0)
                        //{
                        //    //Gửi notify đến người dùng
                        //    try
                        //    {
                        //        msgQueue.ServerMsg.Content = Base64Decode(msgQueue.ServerMsg.Content);
                        //        FirebaseCloudMessage.SendMulticastMessage(_context, msgQueue.ListUsersToSendNotify, msgQueue.ServerMsg).SyncResult();
                        //    }
                        //    catch (Exception ext)
                        //    {
                        //        _logger.LogError(ext.Message, ext);
                        //    }
                        //}

                        //var msgGroup = _context.Rooms.Where(x => x.Uuid == msgQueue.MsgRoomUuid).SingleOrDefault();

                        //if (msgGroup != null)
                        //{
                        //    msgGroup.LastMessageUuid = msgQueue.Uuid;

                        //    var newMsgLine = new Messages
                        //    {
                        //        Uuid = msgQueue.Uuid,
                        //        Content = msgQueue.Content,
                        //        ContentType = msgQueue.ContentType,
                        //        RoomUuid = msgQueue.MsgRoomUuid,
                        //        ReplyMessageUuid = msgQueue.ReplyMsgUuid,
                        //        UserSent = msgQueue.UserSent,
                        //        Status = 1,
                        //        LanguageCode = msgQueue.CountryCode,
                        //        FileInfo = msgQueue.ServerMsg?.FileInfo,
                        //    };

                        //    msgGroup.Status = 1;
                        //    _context.Messages.Add(newMsgLine);
                        //    _context.Rooms.Update(msgGroup);
                        //    _context.SaveChanges();

                        //    //TODO: Add thông tin người xem vào bảng msg_read
                        //}
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }


            }
            finally
            {
                _context.Dispose();
            }

            _timer.Change(TimeLoop, TimeLoop);
        }

        private void SendMessageAsync(object userSent, string v)
        {
            throw new NotImplementedException();
        }
    }
}
