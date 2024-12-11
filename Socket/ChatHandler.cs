using ChatApp.Extensions;
using ChatApp.Firebase;
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
using TWChatAppApiMaster.Utils;
using Microsoft.AspNetCore.Mvc;
using static Google.Apis.Requests.BatchRequest;
using static TWChatAppApiMaster.Models.Response.Admin.GroupGetListResp;
using Microsoft.VisualStudio.Services.Account;
using ChatApp.Timers;
using ChatApp.Utils;
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
            TYPE_CHAT = 0,
            TYPE_TYPING = 1,
            TYPE_READ = 2,
            TYPE_DELETE = 3, // bản tin be: Xoá tất cả, bản tin fe: Xoá tất cả + Xoá chỉ mình tôi
            TYPE_PIN = 4,
            TYPE_CHECK_ONLINE_STATE = 5,
            TYPE_FORWARD = 6,
            TYPE_EDIT = 7,
            TYPE_LIKE_MSG = 8,
            TYPE_LEAVE_GROUP = 9,
            TYPE_JOIN_GROUP = 10,
            TYPE_OTHER_DEVICE_LOGIN = 11,
            TYPE_ERROR = 20,

            TYPE_DELETE_ONLY = 12, // Xoá chỉ mình tôi
            TYPE_BAN = 13, // Cấm chat
            TYPE_UNBAN = 14, // Bỏ cấm chat
            TYPE_HIDE_MSG_USER = 15, // Ẩn tin nhắn của user
            TYPE_DELETE_MSG_USER = 16, // Xoá tin nhắn của user

            TYPE_BLOCK = 17, // Chặn tài khoản
            TYPE_UNBLOCK = 18, // Bỏ chặn tài khoản

            //TYPE_VOICE_CALL = 19, // Gọi thoại
            //TYPE_VIDEO_CALL = 20, // Gọi video

            TYPE_CHANGE_PROFILE = 21, // Bỏ chặn tài khoản
        }

        public ChatHandler(ConnectionManager connectionManager) : base(connectionManager)
        {

        }

        /// <summary>
        /// Nhắn tin
        /// </summary>
        /// <param name="userSent"></param>
        /// <param name="message"></param>
        private async void processClientMessage(string userSent, ClientMessage message)
        {
            var _context = ServiceExtension.GetDbContext();

            try
            {
                TWChatAppApiMaster.Socket.Message data;

                //lấy room chat
                var roomDb = _context.Rooms
                    .AsNoTracking()
                    .Include(x => x.RoomMembers.Where(x => x.InRoom == 1))
                        .ThenInclude(x => x.UserNameNavigation)
                    .Include(x => x.RoomBan.Where(x => x.UserName == userSent && x.State == 1))
                    .Where(x => x.Uuid == message.Receiver)
                    //chỉ lấy nhóm vs usersent trong nhóm
                    .Where(x => x.RoomMembers.Any(x => x.UserName == userSent && x.InRoom == 1))
                    .SingleOrDefault();

                var accDb = _context.Account.AsNoTracking()
                    .Select(x => new
                    {
                        UserName = x.UserName,
                        ActiveState = x.ActiveState,
                        IsEnable = x.IsEnable,
                        Status = x.Status,
                    })
                    .FirstOrDefault(x => x.UserName == userSent);

                //Check acc
                if (accDb == null)
                {
                    data = new TWChatAppApiMaster.Socket.Message
                    {
                        MsgType = (int)MessageType.TYPE_ERROR,
                        Data = EncodeBase64("Tài khoản không tồn tại")
                    };
                    await SendMessageAsync(userSent, JsonConvert.SerializeObject(data));
                    return;
                }

                if (accDb.IsEnable != (int)edAccountState.ACTIVE
                    || accDb.ActiveState != (int)edAccountState.ACTIVE
                    || accDb.Status != (int)edAccountState.ACTIVE)
                {
                    data = new TWChatAppApiMaster.Socket.Message
                    {
                        MsgType = (int)MessageType.TYPE_ERROR,
                        Data = EncodeBase64("Tài khoản của bạn đang bị vô hiệu hoá. Vui lòng liên hệ kỹ thuật để biết thêm chi tiết.")
                    };
                    await SendMessageAsync(userSent, JsonConvert.SerializeObject(data));
                    return;
                }

                if (roomDb != null)
                {
                    String roomName = "";
                    String roomAvatar = "";
                    string ownerUuid = "";
                    List<string> userMembers = new List<string>();

                    var accountMember = roomDb.RoomMembers.First(x => x.UserName == userSent);

                    //xử lý tin nhắn
                    if (message.Type == 2)
                    {
                        // tin nhắn vào nhóm
                        if (roomDb.Status == 4)
                        {
                            data = new TWChatAppApiMaster.Socket.Message
                            {
                                MsgType = (int)MessageType.TYPE_ERROR,
                                Data = EncodeBase64("Bạn không thể gửi tin nhắn vào nhóm này. Vui lòng liên hệ trưởng nhóm để biết thêm chi tiết.")
                            };
                            await SendMessageAsync(userSent, JsonConvert.SerializeObject(data));
                            return;
                        }

                        if (roomDb.RoomBan.Any())
                        {
                            data = new TWChatAppApiMaster.Socket.Message
                            {
                                MsgType = (int)MessageType.TYPE_ERROR,
                                Data = EncodeBase64("Bạn đang bị khoá chat. Vui lòng liên hệ trưởng nhóm để biết thêm chi tiết.")
                            };
                            await SendMessageAsync(userSent, JsonConvert.SerializeObject(data));
                            return;
                        }

                        roomName = roomDb.RoomName ?? "";
                        roomAvatar = roomDb.Avatar ?? "";
                        ownerUuid = roomDb.Uuid;
                        userMembers = roomDb.RoomMembers
                            .Where(x => x.UserName != userSent)
                            .Select(x => x.UserName)
                            .ToList();
                    }
                    else
                    {
                        //Tin nhắn cá nhân
                        var member = roomDb.RoomMembers.First(x => x.UserName != userSent);

                        var friend = await _context.Friends
                            .Where(x => x.UserSent == userSent && x.UserReceiver == member.UserName
                                || x.UserSent == member.UserName && x.UserReceiver == userSent)
                            .Where(x => x.Status == 3)
                            .SingleOrDefaultAsync();

                        if (friend == null && accountMember.UserNameNavigation.RoleId == 3)
                        {
                            data = new TWChatAppApiMaster.Socket.Message
                            {
                                MsgType = (int)MessageType.TYPE_ERROR,
                                Data = EncodeBase64("Bạn không thể gửi tin nhắn cho người này.")
                            };
                            await SendMessageAsync(userSent, JsonConvert.SerializeObject(data));
                            return;
                        }

                        roomName = member.UserNameNavigation.FullName ?? member.UserName;
                        roomAvatar = member.UserNameNavigation.Avatar ?? "";
                        ownerUuid = member.UserNameNavigation.Uuid;
                        userMembers.Add(member.UserName);
                    }

                    var newMsgLineUuid = Guid.NewGuid().ToString();
                    var account = roomDb.RoomMembers.First(x => x.UserName == userSent).UserNameNavigation;

                    //TODO: Gửi thông tin đến nhóm tương ứng
                    var serverMsg = new MessageLineDTO
                    {
                        Uuid = newMsgLineUuid,
                        UserSent = userSent,
                        Content = EncodeBase64(message.Content),
                        ContentType = message.ContentType,
                        LastEdited = DateTime.Now,
                        TimeCreated = DateTime.Now,
                        Status = 1,
                        MsgRoomUuid = roomDb.Uuid,
                        ReplyMsgUuid = message.ReplyMsgUuid,
                        RoomName = EncodeBase64(roomName),
                        RoomAvatar = roomAvatar,
                        Type = message.Type,
                        OwnerUuid = ownerUuid,
                        CountryCode = message.CountryCode,
                        FullName = EncodeBase64(account.FullName ?? account.UserName),
                        Avatar = account.Avatar ?? "",
                        FileInfo = EncodeBase64(message.FileInfo),
                    };

                    if (serverMsg.ContentType == 4)
                    {
                        var file = message.Content.Substring(2, message.Content.Length - 4);
                        var fileName = _context.FilesInfo.Where(x => x.Path == file).Select(x => x.FileName).SingleOrDefault();
                        serverMsg.MediaName = fileName;
                    }

                    if (!string.IsNullOrEmpty(message.ReplyMsgUuid))
                    {
                        var replyMsg = _context.Messages
                            .AsNoTracking()
                            .Include(x => x.UserSentNavigation)
                            .Where(x => x.Uuid == message.ReplyMsgUuid)
                            .SingleOrDefault();

                        if (replyMsg != null)
                        {
                            serverMsg.ReplyMsgUu = new MessageLineDTO
                            {
                                Uuid = replyMsg.Uuid,
                                UserSent = replyMsg.UserSent,
                                Content = EncodeBase64(replyMsg.Content),
                                ContentType = replyMsg.ContentType,
                                MsgRoomUuid = replyMsg.RoomUuid,
                                TimeCreated = replyMsg.TimeCreated,
                                Status = replyMsg.Status,
                                ReplyMsgUuid = replyMsg.ReplyMessageUuid,
                                LastEdited = replyMsg.LastEdited,
                                CountryCode = replyMsg.LanguageCode,
                                FullName = EncodeBase64(replyMsg.UserSentNavigation.FullName ?? replyMsg.UserSent),
                                Avatar = replyMsg.UserSentNavigation.Avatar ?? "",
                            };
                        }
                    }

                    // gửi socket đến người gửi
                    data = new TWChatAppApiMaster.Socket.Message
                    {
                        MsgType = (int)MessageType.TYPE_CHAT,
                        Data = JsonConvert.SerializeObject(serverMsg)
                    };
                    await SendMessageAsync(userSent, JsonConvert.SerializeObject(data));

                    // gửi thông tin đến các member còn lại trong nhóm

                    // chỉnh sửa thông tin nhóm trong trường hợp là nhắn tin p-p khi không gửi cho người gửi
                    // (khi nhắn tin cá nhân tên nhóm là tên của bạn đang nhắn tin cùng)
                    if (message.Type == 1)
                    {
                        var member2 = roomDb.RoomMembers.First(x => x.UserName == userSent);

                        serverMsg.RoomName = EncodeBase64(member2.UserNameNavigation.FullName ?? member2.UserName ?? "");
                        serverMsg.RoomAvatar = member2.UserNameNavigation.Avatar ?? "";
                    }

                    List<string> lstUsersToSendNotify = new List<string>();

                    // Danh sách người dùng online
                    var lstUserOnline = GetAllUsers().Where(x => userMembers.Contains(x)).ToList();

                    // Danh sách người dùng không online
                    var lstUsersOffline = userMembers.Where(x => !lstUserOnline.Contains(x)).ToList();

                    //// Danh sách người dùng để gửi tin nhắn
                    //lstUsersToSendNotify = lstUsersOffline
                    //    .Concat(lstUserOnline)
                    //    .ToList();

                    //Gửi tin nhắn đến người dùng online
                    var responseData = new TWChatAppApiMaster.Socket.Message
                    {
                        MsgType = (int)MessageType.TYPE_CHAT,
                        Data = JsonConvert.SerializeObject(serverMsg)
                    };
                    await SendMessageToGroupUsersAsync(JsonConvert.SerializeObject(responseData), lstUserOnline);

                    //TODO: Chuyển danh sách người dùng không online xử lý độc lập sau
                    var newMsgLine = new MessageChatQueue
                    {
                        Uuid = newMsgLineUuid,
                        MsgRoomUuid = roomDb.Uuid,
                        Content = message.Content,
                        ContentType = message.ContentType,
                        UserSent = userSent,
                        ReplyMsgUuid = message.ReplyMsgUuid,
                        ServerMsg = serverMsg,
                        //ListUsersToSendNotify = lstUsersOffline,
                        ListUsersOnline = lstUserOnline,
                        ListUsersOffline = lstUsersOffline,
                        CountryCode = message.CountryCode,
                        FullName = serverMsg.FullName,
                        Avatar = serverMsg.Avatar,
                        RoomAvatar = serverMsg.RoomAvatar,
                        FileInfo = message.FileInfo,
                    };
                    //MessageQueueManager.enqueue(newMsgLine);

                    HandleMessenge(newMsgLine);
                }
            }
            finally
            {
                _context.Dispose();
            }
        }

        /// <summary>
        /// Hiển thị đang soạn tin nhắn
        /// </summary>
        /// <param name="userSent"></param>
        /// <param name="message"></param>
        private async void processTypingMessage(string userSent, TypingRequest message)
        {
            var _context = ServiceExtension.GetDbContext();
            try
            {
                var roomInfo = await _context.Rooms
                    .AsNoTracking()
                    .Include(x => x.RoomMembers.Where(x => x.InRoom == 1))
                        .ThenInclude(x => x.UserNameNavigation)
                    .Where(x => x.Uuid == message.RoomUuid)
                    .Where(x => x.RoomMembers.Any(x => x.UserName == userSent && x.InRoom == 1))
                    .SingleOrDefaultAsync();

                if (roomInfo != null)
                {
                    var lstUser = roomInfo.RoomMembers.Where(x => x.UserName != userSent).Select(x => x.UserName).ToList();

                    if (lstUser.Count > 0)
                    {
                        var account1 = roomInfo.RoomMembers.First(x => x.UserName == userSent).UserNameNavigation;
                        var data = new TypingResponse
                        {
                            FullName = EncodeBase64(account1.FullName ?? account1.UserName),
                            RoomUuid = message.RoomUuid
                        };
                        var responseData = new TWChatAppApiMaster.Socket.Message
                        {
                            MsgType = (int)MessageType.TYPE_TYPING,
                            Data = JsonConvert.SerializeObject(data)
                        };
                        await SendMessageToGroupUsersAsync(JsonConvert.SerializeObject(responseData), lstUser);
                    }
                }
            }
            catch (Exception e)
            {
                await SendMessageAsync(userSent, e.Message);
            }
            finally
            {
                _context.Dispose();
            }
        }

        /// <summary>
        /// Đọc tin nhắn
        /// </summary>
        /// <param name="userSent"></param>
        /// <param name="request"></param>
        private async void processReadMessage(string userSent, ReadRequest request)
        {
            var _context = ServiceExtension.GetDbContext();
            try
            {
                var message = await _context.Messages
                    .Include(x => x.RoomUu.MessageRead.Where(x => x.UserName == userSent))
                    .Where(x => x.Uuid == request.MsgUuid && x.RoomUuid == request.RoomUuid)
                    .Where(x => x.RoomUu.RoomMembers.Any(x => x.UserName == userSent && x.InRoom == 1))
                    .FirstOrDefaultAsync();

                if (message != null)
                {
                    var read = message.MessageRead.SingleOrDefault();
                    if (read != null)
                    {
                        read.LastMessageId = message.Id;
                    }
                    else
                    {

                        await _context.MessageRead.AddAsync(new MessageRead
                        {
                            RoomUuid = request.RoomUuid,
                            UserName = userSent,
                            LastMessageId = message.Id,
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                //var allRoomInfo = _context.Rooms
                //    .AsNoTracking()
                //    .Include(x => x.RoomMembers.Where(x => x.InRoom == 1))
                //        .ThenInclude(x => x.UserNameNavigation)
                //    .Where(x => x.Uuid == request.RoomUuid)
                //    .Where(x => x.RoomMembers.Any(x => x.UserName == userSent));

                var roomInfo = _context.Rooms
                    .AsNoTracking()
                    .Include(x => x.RoomMembers.Where(x => x.InRoom == 1))
                        .ThenInclude(x => x.UserNameNavigation)
                    .Include(x => x.MessageRead)
                    .Include(x => x.RoomDelete)
                    .Where(x => x.Uuid == request.RoomUuid)
                    .Where(x => x.RoomMembers.Any(x => x.UserName == userSent && x.InRoom == 1))
                    .SingleOrDefault();

                if (roomInfo != null)
                {
                    var lstUser = roomInfo.RoomMembers.Select(x => x.UserName).ToList();

                    if (lstUser.Count > 0)
                    {
                        var data = new ReadResponse
                        {
                            UserName = userSent,
                            RoomUuid = request.RoomUuid,
                            MsgUuid = request.MsgUuid,
                        };

                        foreach (var user in lstUser)
                        {
                            data.UnReadCount = MessengeExtension.GetUnreadCount(
                                _context,
                                user,
                                request.RoomUuid,
                                roomInfo.Messages,
                                roomInfo.MessageRead,
                                roomInfo.RoomDelete
                            );
                            data.UnReadTotal = await MessengeExtension.GetUnreadTotal(_context, user);

                            var responseData = new TWChatAppApiMaster.Socket.Message
                            {
                                MsgType = (int)MessageType.TYPE_READ,
                                Data = JsonConvert.SerializeObject(data)
                            };

                            SendMessageAsync(user, JsonConvert.SerializeObject(responseData));
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
            finally
            {
                _context.Dispose();
            }
        }

        /// <summary>
        /// Xoá tin nhắn
        /// </summary>
        /// <param name="userSent"></param>
        /// <param name="request"></param>
        /// <param name="type">1 = Xoá với tất cả; 2 = Xoá chỉ mình tôi</param>
        private async void processDeleteMessage(string userSent, DeleteRequest request, sbyte type)
        {
            var _context = ServiceExtension.GetDbContext();
            try
            {
                var roomAvailable = await MessengeExtension.GetRoomAvailable(_context, userSent, true, request.RoomUuid);
                var roomInfo = await roomAvailable
                    //.AsNoTracking()
                    .AsSplitQuery()
                    .Include(x => x.RoomMembers)
                        .ThenInclude(x => x.UserNameNavigation)
                    .Include(x => x.MessageRead)
                    .Include(x => x.RoomDelete)
                    .Include(x => x.CreaterNavigation)
                    .Include(x => x.RoomPin)
                    .SingleOrDefaultAsync();

                if (roomInfo == null) return;

                var messageLst = await _context.Messages
                    .Where(x => !x.MessageDelete.Any(x => x.UserName == userSent))
                    .Where(x => request.ListMsgUuid.Contains(x.Uuid))
                    .Where(x => x.RoomUuid == request.RoomUuid)
                    .ToListAsync();

                var lstUser = new List<string>();
                var roomMembers = roomInfo.RoomMembers.Where(x => x.InRoom == 1);
                if (type == 1) // Xoá với mọi thành viên
                {
                    var memberSent = roomMembers.First(x => x.UserName == userSent);
                    var isAllow = await GroupService.CheckPermission(request.RoomUuid, userSent, edTypeGroupPermisson.DELETE_MESSAGE);// Kiểm tra có được gắn chức năng này không
                    if (messageLst.Any(x => x.UserSent != userSent) && memberSent.RoleId == 3 && !isAllow)
                    {
                        var data = new TWChatAppApiMaster.Socket.Message
                        {
                            MsgType = (int)MessageType.TYPE_ERROR,
                            Data = EncodeBase64("Bạn không thể xoá tin nhắn trong nhóm. Vui lòng liên hệ trưởng nhóm để biết thêm chi tiết.")
                        };
                        await SendMessageAsync(userSent, JsonConvert.SerializeObject(data));
                        return;
                    }

                    messageLst.ForEach(x => x.Status = 4);
                    await _context.SaveChangesAsync();

                    lstUser = roomMembers.Select(x => x.UserName).ToList();
                }
                else // Xoá với mình tôi
                {
                    var nemDeleteMsg = messageLst
                        .Select(x => new MessageDelete
                        {
                            MessageUuid = x.Uuid,
                            UserName = userSent,
                        })
                        .ToList();

                    await _context.MessageDelete.AddRangeAsync(nemDeleteMsg);
                    await _context.SaveChangesAsync();

                    lstUser.Add(userSent);
                }

                if (lstUser.Count > 0)
                {
                    var data = new DeleteResponse
                    {
                        UserName = userSent,
                        RoomUuid = request.RoomUuid,
                        ListMsgUuid = request.ListMsgUuid,
                        DeviceId = request.DeviceId,
                    };

                    var lastMessenge = await MessengeExtension.GetLastMessengeInRoom(_context, request.RoomUuid, userSent);

                    data.LastMessenge = new MessagesGroupDTO
                    {
                        Id = roomInfo.Id,
                        Uuid = roomInfo.Uuid,
                        Type = roomInfo.Type,
                        LastUpdated = roomInfo.LastUpdated,
                        TimeCreated = roomInfo.TimeCreated,
                        Pinned = roomInfo.RoomPin.Any(rp => rp.UserName == userSent && rp.State == 1),
                        CreatorFullName = MessengeExtension.EncodeBase64(roomInfo.CreaterNavigation?.FullName ?? ""),
                    };

                    if (lastMessenge != null)
                    {
                        var memberSent = roomMembers.Select(x => x.UserNameNavigation).FirstOrDefault(x => x.UserName == userSent);

                        data.LastMessenge.Content = MessengeExtension.EncodeBase64(lastMessenge.Content);
                        data.LastMessenge.ContentType = lastMessenge.ContentType;
                        data.LastMessenge.ForwardFrom = lastMessenge.ForwardFrom ?? "";
                        data.LastMessenge.FullName = MessengeExtension.EncodeBase64(memberSent?.FullName ?? memberSent?.UserName ?? "");
                        data.LastMessenge.Uuid = lastMessenge.Uuid;
                        data.LastMessenge.LastMsgLineUuid = lastMessenge.Uuid;
                        data.LastMessenge.ReadCounter = lastMessenge.MessageRead.Count(mr => mr.UserName != userSent);
                        data.LastMessenge.LikeCount = lastMessenge.MessageLike.Count();
                        data.LastMessenge.UserSent = lastMessenge.UserSent;
                        data.LastMessenge.UserSentIsBlock = lastMessenge.UserSentNavigation.RoomBlock.SingleOrDefault()?.State ?? 0;
                        data.LastMessenge.Status = lastMessenge.Status;
                        data.LastMessenge.IsDeleteWithMe = lastMessenge.MessageDelete.Any(x2 => x2.UserName == userSent);
                        data.LastMessenge.LastUpdated = lastMessenge.LastEdited;
                        data.LastMessenge.FileInfo = lastMessenge.FileInfo != null ? MessengeExtension.EncodeBase64(lastMessenge.FileInfo) : null;
                    }
                    else if (roomInfo.Type == 2)
                    {
                        data.LastMessenge.UserSent = "admin";
                        data.LastMessenge.Content = MessengeExtension.EncodeBase64(roomInfo.CreaterNavigation.FullName ?? roomInfo.CreaterNavigation.UserName);
                        data.LastMessenge.ContentType = 1;
                        data.LastMessenge.UnreadCount = 0;
                        data.LastMessenge.LastUpdated = roomInfo.TimeCreated;
                    }

                    if (roomInfo.Type == 1)
                    {
                        var memberChatWith = roomMembers.FirstOrDefault(rm => rm.UserName != userSent)?.UserNameNavigation;
                        data.LastMessenge.Avatar = memberChatWith?.Avatar ?? "";
                        data.LastMessenge.OwnerUuid = roomInfo.CreaterNavigation.Uuid;
                        data.LastMessenge.PartnerUuid = memberChatWith?.Uuid ?? "";
                        data.LastMessenge.OwnerName = MessengeExtension.EncodeBase64(roomInfo.CreaterNavigation.FullName ?? roomInfo.CreaterNavigation.UserName);
                        data.LastMessenge.ShowName = MessengeExtension.EncodeBase64(memberChatWith?.FullName ?? memberChatWith?.UserName);
                        data.LastMessenge.ShowUuid = memberChatWith?.Uuid;
                    }
                    else
                    {
                        data.LastMessenge.Avatar = roomInfo.Avatar ?? "";
                        data.LastMessenge.OwnerUuid = roomInfo.Uuid;
                        data.LastMessenge.OwnerName = MessengeExtension.EncodeBase64(roomInfo.RoomName ?? "");
                        data.LastMessenge.ShowName = MessengeExtension.EncodeBase64(roomInfo.RoomName ?? "");
                        data.LastMessenge.ShowUuid = roomInfo.Uuid;
                    }

                    foreach (var user in lstUser)
                    {
                        if(type == 1)
                        {
                            data.UnReadCount = MessengeExtension.GetUnreadCount(
                                _context,
                                user,
                                request.RoomUuid,
                                roomInfo.Messages,
                                roomInfo.MessageRead,
                                roomInfo.RoomDelete
                            );
                        }

                        //data.UnReadTotal = await MessengeExtension.GetUnreadTotal(_context, user);

                        var responseData = new TWChatAppApiMaster.Socket.Message
                        {
                            MsgType = type == 1 ? (int)MessageType.TYPE_DELETE : (int)MessageType.TYPE_DELETE_ONLY,
                            Data = JsonConvert.SerializeObject(data)
                        };

                        SendMessageAsync(user, JsonConvert.SerializeObject(responseData));
                    }



                    //data.UnReadCount = MessengeExtension.GetUnreadCount(
                    //            _context,
                    //            user,
                    //            request.RoomUuid,
                    //            roomInfo.Messages,
                    //            roomInfo.MessageRead,
                    //            roomInfo.RoomDelete
                    //        );
                    //data.UnReadTotal = await MessengeExtension.GetUnreadTotal(_context, user);

                    //var responseData = new TWChatAppApiMaster.Socket.Message
                    //{
                    //    MsgType = type == 1 ? (int)MessageType.TYPE_DELETE : (int)MessageType.TYPE_DELETE_ONLY,
                    //    Data = JsonConvert.SerializeObject(data)
                    //};
                    //await SendMessageToGroupUsersAsync(JsonConvert.SerializeObject(responseData), lstUser);
                }
            }
            catch (Exception e)
            {

            }
            finally
            {
                _context.Dispose();
            }
        }

        /// <summary>
        /// Ghim tin nhắn
        /// </summary>
        /// <param name="userSent"></param>
        /// <param name="request"></param>
        private async void processPinMessage(string userSent, PinRequest request)
        {
            var _context = ServiceExtension.GetDbContext();
            try
            {
                var roomInfo = await _context.Rooms
                    .Include(x => x.RoomMembers.Where(x => x.InRoom == 1))
                        .ThenInclude(x => x.UserNameNavigation)
                    .Include(x => x.MessagePin)
                    .Where(x => x.Uuid == request.RoomUuid)
                    .Where(x => x.RoomMembers.Any(x => x.UserName == userSent && x.InRoom == 1))
                    .SingleOrDefaultAsync();

                if (roomInfo == null) return;

                var lstPin = roomInfo.MessagePin.ToList();
                if (request.State == 0)
                {
                    lstPin = lstPin.Where(x => request.LstMsgUuid.Contains(x.MessageUuid)).ToList();
                    lstPin.ForEach(x => x.State = 0);
                }
                else
                {
                    // Danh sách uuid mới
                    var exceptUuids = request.LstMsgUuid.Except(lstPin.Select(x => x.MessageUuid)).ToList();

                    // Update pin tồn tại
                    lstPin = lstPin.Where(x => request.LstMsgUuid.Contains(x.MessageUuid)).ToList();
                    lstPin.ForEach(x => x.State = 1);
                    lstPin.ForEach(x => x.TimePin = DateTime.Now);

                    // Thêm pin với uuid mới
                    var newPin = exceptUuids
                        .Select(x => new MessagePin
                        {
                            MessageUuid = x,
                            RoomUuid = roomInfo.Uuid,
                            State = 1,
                            UserName = userSent,
                            TimePin = DateTime.Now,
                        })
                        .ToList();
                    await _context.MessagePin.AddRangeAsync(newPin);
                }
                await _context.SaveChangesAsync();

                var lstUser = roomInfo.RoomMembers.Select(x => x.UserName).ToList();
                if (lstUser.Count > 0)
                {
                    var data = new PinResponse
                    {
                        RoomUuid = request.RoomUuid,
                        UserName = userSent,
                        State = request.State,
                        LstMsgUuid = request.LstMsgUuid
                    };
                    var responseData = new TWChatAppApiMaster.Socket.Message
                    {
                        MsgType = (int)MessageType.TYPE_PIN,
                        Data = JsonConvert.SerializeObject(data)
                    };
                    await SendMessageToGroupUsersAsync(JsonConvert.SerializeObject(responseData), lstUser);
                }
            }
            catch (Exception e)
            {

            }
            finally
            {
                _context.Dispose();
            }
        }

        /// <summary>
        /// Chuyển tiếp tin nhắn
        /// </summary>
        /// <param name="userSent"></param>
        /// <param name="request"></param>
        private async void processForwardMessage(string userSent, ForwardMessageRequest request)
        {
            var _context = ServiceExtension.GetDbContext();
            try
            {
                var msgLine = await _context.Messages
                    .AsNoTracking()
                    .Include(x => x.UserSentNavigation)
                    .Where(x => x.Uuid == request.MsgLineUuid)
                    .SingleOrDefaultAsync();

                var roomInfo = await _context.Rooms
                    .Include(x => x.RoomMembers.Where(x => x.InRoom == 1))
                        .ThenInclude(x => x.UserNameNavigation)
                    .Where(x => x.Uuid == request.RoomUuid)
                    .Where(x => x.RoomMembers.Any(x => x.UserName == userSent && x.InRoom == 1))
                    .SingleOrDefaultAsync();

                if (msgLine == null || roomInfo == null)
                {
                    var data1 = new TWChatAppApiMaster.Socket.Message
                    {
                        MsgType = (int)MessageType.TYPE_ERROR,
                        Data = EncodeBase64("Đã có lỗi xảy ra. Gửi tin nhắn không thành công")
                    };
                    await SendMessageAsync(userSent, JsonConvert.SerializeObject(data1));
                    return;
                }

                var accountSend = msgLine.UserSentNavigation;
                var newMsgLineUuid = Guid.NewGuid().ToString();

                await _context.Messages.AddAsync(new Messages
                {
                    Uuid = newMsgLineUuid,
                    Content = msgLine.Content,
                    ContentType = msgLine.ContentType,
                    RoomUuid = request.RoomUuid,
                    TimeCreated = DateTime.Now,
                    LastEdited = DateTime.Now,
                    UserSent = userSent,
                    Status = 1,
                    LanguageCode = msgLine.LanguageCode,
                    ForwardFrom = accountSend.FullName,
                    FileInfo = msgLine.FileInfo,
                });

                roomInfo.LastMessageUuid = newMsgLineUuid;
                await _context.SaveChangesAsync();

                var lstUser = roomInfo.RoomMembers.Select(x => x.UserName).ToList();
                if (lstUser.Count > 0)
                {
                    String roomName = "";
                    String roomAvatar = "";
                    String ownerUuid = "";
                    if (roomInfo.Type == 2)
                    {
                        roomName = roomInfo.RoomName ?? "";
                        roomAvatar = roomInfo.Avatar ?? "";
                        ownerUuid = roomInfo.Uuid;
                    }
                    else
                    {
                        var member = roomInfo.RoomMembers.First(x => x.UserName != userSent).UserNameNavigation;
                        roomName = member.FullName ?? member.UserName;
                        roomAvatar = member.Avatar ?? "";
                        ownerUuid = member.Uuid;
                    }

                    var accountInfo = roomInfo.RoomMembers.First(x => x.UserName == userSent).UserNameNavigation;
                    var serverMsg = new MessageLineDTO
                    {
                        Uuid = newMsgLineUuid,
                        UserSent = userSent,
                        Content = EncodeBase64(msgLine.Content),
                        ContentType = msgLine.ContentType,
                        LastEdited = msgLine.LastEdited,
                        TimeCreated = msgLine.TimeCreated,
                        Status = 1,
                        MsgRoomUuid = request.RoomUuid,
                        ReplyMsgUuid = null,
                        RoomName = EncodeBase64(roomName),
                        Type = roomInfo.Type,
                        OwnerUuid = ownerUuid,
                        CountryCode = msgLine.LanguageCode,
                        FullName = EncodeBase64(accountInfo.FullName ?? accountInfo.UserName),
                        Avatar = accountInfo.Avatar ?? "",
                        ForwardFrom = EncodeBase64(accountSend.FullName ?? accountSend.UserName),
                        RoomAvatar = roomAvatar,
                        FileInfo = msgLine.FileInfo != null ? EncodeBase64(msgLine.FileInfo) : null,
                        TimeForward = DateTime.Now,
                    };

                    var data = new TWChatAppApiMaster.Socket.Message
                    {
                        MsgType = (int)MessageType.TYPE_FORWARD,
                        Data = JsonConvert.SerializeObject(serverMsg)
                    };
                    await SendMessageToGroupUsersAsync(JsonConvert.SerializeObject(data), lstUser);

                    lstUser.Remove(userSent);
                    var lstToSend = new List<string>();
                    foreach (var item in lstUser)
                    {
                        if (!CheckUserIsOnline(item))
                        {
                            lstToSend.Add(item);
                        }
                    }

                    FirebaseCloudMessage.SendMulticastMessage(_context, lstToSend, serverMsg).SyncResult();

                    //TODO: Add thông tin người xem vào bảng msg_read
                    ChatHandler.getInstance().processReadMessage(serverMsg.UserSent, new ReadRequest
                    {
                        MsgUuid = serverMsg.Uuid,
                        RoomUuid = serverMsg.MsgRoomUuid,
                    });
                }
            }
            catch (Exception e)
            {
            }
            finally
            {
                _context.Dispose();
            }
        }

        /// <summary>
        /// Sửa tin nhắn
        /// </summary>
        /// <param name="userSent"></param>
        /// <param name="request"></param>
        private async void processEditMessage(string userSent, EditMessageRequest request)
        {
            var _context = ServiceExtension.GetDbContext();
            try
            {
                var msgLine = await _context.Messages
                    .Where(x => x.Uuid == request.MsgLineUuid)
                    .SingleOrDefaultAsync();

                if (msgLine == null)
                {
                    var data1 = new TWChatAppApiMaster.Socket.Message
                    {
                        MsgType = (int)MessageType.TYPE_ERROR,
                        Data = EncodeBase64("Đã có lỗi xảy ra. Sửa tin nhắn không thành công")
                    };
                    await SendMessageAsync(userSent, JsonConvert.SerializeObject(data1));
                    return;
                }

                var roomInfo = await _context.Rooms
                    .AsNoTracking()
                    .Include(x => x.RoomMembers.Where(x => x.InRoom == 1))
                        .ThenInclude(x => x.UserNameNavigation)
                    .Where(x => x.Uuid == msgLine.RoomUuid)
                    .Where(x => x.RoomMembers.Any(x => x.UserName == userSent && x.InRoom == 1))
                    .SingleOrDefaultAsync();

                if (msgLine.UserSent != userSent || roomInfo == null)
                {
                    var data1 = new TWChatAppApiMaster.Socket.Message
                    {
                        MsgType = (int)MessageType.TYPE_ERROR,
                        Data = EncodeBase64("Bạn không thể sửa tin nhắn trong nhóm. Vui lòng liên hệ trưởng nhóm để biết thêm chi tiết.")
                    };
                    await SendMessageAsync(userSent, JsonConvert.SerializeObject(data1));
                    return;
                }

                msgLine.Content = request.Content;
                msgLine.Status = 2;
                await _context.SaveChangesAsync();

                var lstUser = roomInfo.RoomMembers.Select(x => x.UserName).ToList();

                var data = new EditMessageRequest
                {
                    RoomUuid = roomInfo.Uuid,
                    MsgLineUuid = request.MsgLineUuid,
                    Content = EncodeBase64(request.Content),
                };
                var responseData = new TWChatAppApiMaster.Socket.Message
                {
                    MsgType = (int)MessageType.TYPE_EDIT,
                    Data = JsonConvert.SerializeObject(data)
                };
                await SendMessageToGroupUsersAsync(JsonConvert.SerializeObject(responseData), lstUser);
            }
            catch (Exception e)
            {

            }
            finally
            {
                _context.Dispose();
            }
        }

        /// <summary>
        /// Thả tương tác tin nhắn
        /// </summary>
        /// <param name="userSent"></param>
        /// <param name="request"></param>
        private async void processLikeMessage(string userSent, LikeMessage request)
        {
            var _context = ServiceExtension.GetDbContext();
            try
            {
                var msgLine = await _context.Messages
                    .Include(x => x.MessageLike.Where(x => x.UserName == userSent))
                    .Where(x => x.Uuid == request.MsgLineUuid)
                    .SingleOrDefaultAsync();

                if (msgLine == null)
                {
                    var data1 = new TWChatAppApiMaster.Socket.Message
                    {
                        MsgType = (int)MessageType.TYPE_ERROR,
                        Data = EncodeBase64("Đã có lỗi xảy ra. Bạn không thể tương tác tin nhắn này")
                    };
                    await SendMessageAsync(userSent, JsonConvert.SerializeObject(data1));
                    return;
                }

                var roomInfo = await _context.Rooms
                    .AsNoTracking()
                    .Include(x => x.RoomMembers.Where(x => x.InRoom == 1))
                        .ThenInclude(x => x.UserNameNavigation)
                    .Where(x => x.Uuid == msgLine.RoomUuid)
                    .Where(x => x.RoomMembers.Any(x => x.UserName == userSent && x.InRoom == 1))
                    .SingleOrDefaultAsync();

                if (roomInfo == null)
                {
                    var data1 = new TWChatAppApiMaster.Socket.Message
                    {
                        MsgType = (int)MessageType.TYPE_ERROR,
                        Data = EncodeBase64("Bạn không thể sửa tin nhắn trong nhóm. Vui lòng liên hệ trưởng nhóm để biết thêm chi tiết.")
                    };
                    await SendMessageAsync(userSent, JsonConvert.SerializeObject(data1));
                    return;
                }

                var likeEmoji = msgLine.MessageLike.FirstOrDefault(x => x.Type == request.Type);

                if (request.Status == 1)
                {
                    if (likeEmoji == null)
                    {
                        await _context.MessageLike.AddAsync(new MessageLike
                        {
                            MessageUuid = msgLine.Uuid,
                            Type = request.Type,
                            UserName = userSent,
                        });
                    }
                    else
                    {
                        likeEmoji.Status = 1;
                    }
                }
                else
                {
                    if (likeEmoji != null)
                        likeEmoji.Status = 0;
                }

                await _context.SaveChangesAsync();

                var lstUser = roomInfo.RoomMembers.Select(x => x.UserName).ToList();
                var userSentDB = roomInfo.RoomMembers.First(x => x.UserName == userSent).UserNameNavigation;
                var data = new LikeMessageData
                {
                    MsgLineUuid = request.MsgLineUuid,
                    Type = request.Type,
                    Status = request.Status,
                    Uuid = userSentDB.Uuid,
                    UserName = userSentDB.UserName,
                    FullName = EncodeBase64(userSentDB.FullName ?? userSentDB.UserName),
                    Avatar = userSentDB.Avatar,
                    TimeCreated = DateTime.Now,
                };
                var responseData = new TWChatAppApiMaster.Socket.Message
                {
                    MsgType = (int)MessageType.TYPE_LIKE_MSG,
                    Data = JsonConvert.SerializeObject(data)
                };
                await SendMessageToGroupUsersAsync(JsonConvert.SerializeObject(responseData), lstUser);
            }
            catch (Exception e)
            {

            }
            finally
            {
                _context.Dispose();
            }
        }

        /// <summary>
        /// Kiểm tra online
        /// </summary>
        /// <param name="userSent"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private async Task checkOnlineState(string userSent, string data)
        {
            var _context = ServiceExtension.GetDbContext();
            try
            {
                var param = System.Text.Json.JsonSerializer.Deserialize<OnlineStateRequest>(data);

                var lstValue = new List<OnlineStateInfo>();
                foreach (var item in param.LstUser)
                {

                    var username = _context.Account.Where(x => x.Uuid == item).Select(x => x.UserName).SingleOrDefault();
                    if (username != null)
                    {
                        var accountDb = _context.Account.FirstOrDefault(x => x.UserName == username);
                        var timeOff = accountDb.Session.LastOrDefault(x => x.IsOnline == 0)?.TimeDisconnectSocket ?? null;
                        bool state = CheckUserIsOnline(username);
                        lstValue.Add(new OnlineStateInfo { UserName = item, OnlineState = state, TimeOff = timeOff });
                    }
                }
                if (lstValue.Count() > 0)
                {
                    var responseData = new TWChatAppApiMaster.Socket.Message
                    {
                        MsgType = (int)MessageType.TYPE_CHECK_ONLINE_STATE,
                        Data = JsonConvert.SerializeObject(lstValue)
                    };
                    await SendMessageAsync(userSent, JsonConvert.SerializeObject(responseData));
                }

            }
            catch (Exception e)
            {
                SendMessageAsync(userSent, e.Message);
            }
            finally { _context.Dispose(); }
        }

        /// <summary>
        /// Cấm chat
        /// </summary>
        /// <param name="userSent"></param>
        /// <param name="request"></param>
        private async void processBanMessage(string userSent, BanMessage request)
        {
            var _context = ServiceExtension.GetDbContext();
            try
            {
                var roomInfo = await _context.Rooms
                    .AsNoTracking()
                    .Include(x => x.RoomMembers)
                    .Where(x => x.Type == 2)
                    .Where(x => x.Uuid == request.RoomUuid)
                    .Where(x => x.RoomMembers.Any(x => x.UserName == userSent && x.InRoom == 1)
                        && x.RoomMembers.Any(x => x.UserName == request.UserName && x.InRoom == 1))
                    .SingleOrDefaultAsync();

                var roomBan = await _context.RoomBan
                    .FirstOrDefaultAsync(x => x.UserName == request.UserName && x.RoomUuid == request.RoomUuid);

                var isAllow = await GroupService.CheckPermission(request.RoomUuid, userSent, edTypeGroupPermisson.LOCK_MEMBER);// Kiểm tra có được gắn chức năng này không
                if (roomInfo == null
                    || roomInfo.RoomMembers.Any(x => x.UserName == userSent && x.RoleId != 1) && !isAllow
                    || roomBan != null && roomBan.State == 1)
                {
                    var data1 = new TWChatAppApiMaster.Socket.Message
                    {
                        MsgType = (int)MessageType.TYPE_ERROR,
                        Data = EncodeBase64("Đã có lỗi xảy ra. Bạn không thể cấm chat người dùng này")
                    };
                    await SendMessageAsync(userSent, JsonConvert.SerializeObject(data1));
                    return;
                }

                if (roomBan != null)
                {
                    roomBan.State = 1;
                }
                else
                {
                    await _context.RoomBan.AddAsync(new RoomBan
                    {
                        RoomUuid = request.RoomUuid,
                        UserName = request.UserName,
                    });
                }
                await _context.SaveChangesAsync();

                //var lstUser = new List<string>() { userSent, request.UserName };
                var lstUser = roomInfo.RoomMembers.Where(x => x.InRoom == 1).Select(x => x.UserName).ToList();
                var responseData = new TWChatAppApiMaster.Socket.Message
                {
                    MsgType = (int)MessageType.TYPE_BAN,
                    Data = JsonConvert.SerializeObject(request)
                };
                await SendMessageToGroupUsersAsync(JsonConvert.SerializeObject(responseData), lstUser);
            }
            catch (Exception e)
            {

            }
            finally
            {
                _context.Dispose();
            }
        }

        /// <summary>
        /// Bỏ cấm chat
        /// </summary>
        /// <param name="userSent"></param>
        /// <param name="request"></param>
        private async void processUnBanMessage(string userSent, UnBanMessage request)
        {
            var _context = ServiceExtension.GetDbContext();
            try
            {
                var roomInfo = await _context.Rooms
                    .AsNoTracking()
                    .Include(x => x.RoomMembers)
                    .Where(x => x.Type == 2)
                    .Where(x => x.Uuid == request.RoomUuid)
                    .Where(x => x.RoomMembers.Any(x => x.UserName == userSent && x.InRoom == 1)
                        && x.RoomMembers.Any(x => x.UserName == request.UserName && x.InRoom == 1))
                    .SingleOrDefaultAsync();

                var roomBan = await _context.RoomBan
                    .FirstOrDefaultAsync(x => x.UserName == request.UserName && x.RoomUuid == request.RoomUuid);

                var isAllow = await GroupService.CheckPermission(request.RoomUuid, userSent, edTypeGroupPermisson.LOCK_MEMBER);// Kiểm tra có được gắn chức năng này không
                if (roomInfo == null
                    || roomInfo.RoomMembers.Any(x => x.UserName == userSent && x.RoleId != 1) && !isAllow
                    || roomBan == null
                    || roomBan.State == 0)
                {
                    var data1 = new TWChatAppApiMaster.Socket.Message
                    {
                        MsgType = (int)MessageType.TYPE_ERROR,
                        Data = EncodeBase64("Đã có lỗi xảy ra. Bạn không thể bỏ cấm chat người dùng này")
                    };
                    await SendMessageAsync(userSent, JsonConvert.SerializeObject(data1));
                    return;
                }

                roomBan.State = 0;
                await _context.SaveChangesAsync();

                //var lstUser = new List<string>() { userSent, request.UserName };
                var lstUser = roomInfo.RoomMembers.Where(x => x.InRoom == 1).Select(x => x.UserName).ToList();
                var responseData = new TWChatAppApiMaster.Socket.Message
                {
                    MsgType = (int)MessageType.TYPE_UNBAN,
                    Data = JsonConvert.SerializeObject(request)
                };
                await SendMessageToGroupUsersAsync(JsonConvert.SerializeObject(responseData), lstUser);
            }
            catch (Exception e)
            {

            }
            finally
            {
                _context.Dispose();
            }
        }

        /// <summary>
        /// Ẩn tin nhắn của user
        /// </summary>
        /// <param name="userSent"></param>
        /// <param name="request"></param>
        private async void processHideMessageUser(string userSent, HideMessageUser request)
        {
            var _context = ServiceExtension.GetDbContext();
            try
            {
                var roomInfo = await _context.Rooms
                    .AsNoTracking()
                    .Include(x => x.RoomMembers.Where(x => x.InRoom == 1))
                    .Where(x => x.Type == 2)
                    .Where(x => x.Uuid == request.RoomUuid)
                    .Where(x => x.RoomMembers.Any(x => x.UserName == userSent && x.InRoom == 1)
                        && x.RoomMembers.Any(x => x.UserName == request.UserName && x.InRoom == 1))
                    .SingleOrDefaultAsync();

                if (roomInfo == null
                    || roomInfo.RoomMembers.First(x => x.UserName == userSent).RoleId != 1)
                {
                    var data1 = new TWChatAppApiMaster.Socket.Message
                    {
                        MsgType = (int)MessageType.TYPE_ERROR,
                        Data = EncodeBase64("Đã có lỗi xảy ra. Bạn không thể ẩn tin nhắn người dùng này")
                    };
                    await SendMessageAsync(userSent, JsonConvert.SerializeObject(data1));
                    return;
                }

                var messageLst = await _context.Messages
                    .Where(x => x.UserSent == request.UserName && x.RoomUuid == request.RoomUuid)
                    .Where(x => x.Status != 4 && x.Status != 3)
                    .ToListAsync();

                messageLst.ForEach(x => x.Status = 3);
                await _context.SaveChangesAsync();

                var lstUser = roomInfo.RoomMembers.Select(x => x.UserName).ToList();
                var responseData = new TWChatAppApiMaster.Socket.Message
                {
                    MsgType = (int)MessageType.TYPE_HIDE_MSG_USER,
                    Data = JsonConvert.SerializeObject(request)
                };
                await SendMessageToGroupUsersAsync(JsonConvert.SerializeObject(responseData), lstUser);
            }
            catch (Exception e)
            {

            }
            finally
            {
                _context.Dispose();
            }
        }

        /// <summary>
        /// Xoá tin nhắn của user
        /// </summary>
        /// <param name="userSent"></param>
        /// <param name="request"></param>
        private async void processDeleteMessageUser(string userSent, DeleteMessageUser request)
        {
            var _context = ServiceExtension.GetDbContext();
            try
            {
                var roomInfo = await _context.Rooms
                    .AsNoTracking()
                    .Include(x => x.RoomMembers.Where(x => x.InRoom == 1))
                    .Where(x => x.Type == 2)
                    .Where(x => x.Uuid == request.RoomUuid)
                    .Where(x => x.RoomMembers.Any(x => x.UserName == userSent && x.InRoom == 1)
                        && x.RoomMembers.Any(x => x.UserName == request.UserName && x.InRoom == 1))
                    .SingleOrDefaultAsync();

                if (roomInfo == null
                    || roomInfo.RoomMembers.First(x => x.UserName == userSent).RoleId != 1)
                {
                    var data1 = new TWChatAppApiMaster.Socket.Message
                    {
                        MsgType = (int)MessageType.TYPE_ERROR,
                        Data = EncodeBase64("Đã có lỗi xảy ra. Bạn không thể xoá tin nhắn người dùng này")
                    };
                    await SendMessageAsync(userSent, JsonConvert.SerializeObject(data1));
                    return;
                }

                var messageLst = await _context.Messages
                    .Where(x => x.UserSent == request.UserName && x.RoomUuid == request.RoomUuid)
                    .Where(x => x.Status != 4)
                    .ToListAsync();

                messageLst.ForEach(x => x.Status = 4);
                await _context.SaveChangesAsync();

                var lstUser = roomInfo.RoomMembers.Select(x => x.UserName).ToList();
                var responseData = new TWChatAppApiMaster.Socket.Message
                {
                    MsgType = (int)MessageType.TYPE_DELETE_MSG_USER,
                    Data = JsonConvert.SerializeObject(request)
                };
                await SendMessageToGroupUsersAsync(JsonConvert.SerializeObject(responseData), lstUser);
            }
            catch (Exception e)
            {

            }
            finally
            {
                _context.Dispose();
            }
        }

        /// <summary>
        /// Chặn tài khoản
        /// </summary>
        /// <param name="userSent"></param>
        /// <param name="request"></param>
        private async void processBlockMessage(string userSent, BlockMessage request)
        {
            var _context = ServiceExtension.GetDbContext();
            try
            {
                var roomInfo = await _context.Rooms
                    .AsNoTracking()
                    .Include(x => x.RoomMembers)
                    .Where(x => x.Type == 2)
                    .Where(x => x.Uuid == request.RoomUuid)
                    .Where(x => x.RoomMembers.Any(x => x.UserName == userSent && x.InRoom == 1)
                        && x.RoomMembers.Any(x => x.UserName == request.UserName && x.InRoom == 1))
                    .SingleOrDefaultAsync();

                var roomBlock = await _context.RoomBlock
                    .FirstOrDefaultAsync(x => x.UserName == request.UserName && x.RoomUuid == request.RoomUuid);

                var isAllow = await GroupService.CheckPermission(request.RoomUuid, request.UserName, edTypeGroupPermisson.BLOCK_MEMBER);// Kiểm tra có được gắn chức năng này không

                if (roomInfo == null
                    || (roomInfo.RoomMembers.Any(x => x.UserName == userSent && x.RoleId != 1) && !isAllow)
                    || roomBlock != null && roomBlock.State == (int)edBlockState.BLOCK)
                {
                    var data1 = new TWChatAppApiMaster.Socket.Message
                    {
                        MsgType = (int)MessageType.TYPE_ERROR,
                        Data = EncodeBase64("Đã có lỗi xảy ra. Bạn không thể chặn người dùng này")
                    };
                    await SendMessageAsync(userSent, JsonConvert.SerializeObject(data1));
                    return;
                }

                //Thêm ds block
                if (roomBlock != null)
                {
                    roomBlock.State = (int)edBlockState.BLOCK;
                }
                else
                {
                    await _context.RoomBlock.AddAsync(new RoomBlock
                    {
                        RoomUuid = request.RoomUuid,
                        UserName = request.UserName,
                    });
                }
                // Xoá thành viên
                var member = roomInfo.RoomMembers.FirstOrDefault(x => x.UserName == request.UserName);

                if (member != null && member.InRoom == 1)
                {
                    member.InRoom = 0;
                    member.ChangeGroupInfo = 0;
                    member.DeleteMessage = 0;
                    member.LockMember = 0;
                    member.AddMember = 0;
                    member.BanUser = 0;
                    _context.RoomMembers.Update(member);

                    var roomPinDb = await _context.RoomPin.Where(x => x.RoomUuid == request.RoomUuid && x.UserName == member.UserName).FirstOrDefaultAsync();
                    if (roomPinDb != null && roomPinDb.State == 1)
                    {
                        roomPinDb.State = 0;
                    }
                }

                await _context.SaveChangesAsync();

                //var lstUser = new List<string>() { userSent, request.UserName };
                var lstUser = roomInfo.RoomMembers.Select(x => x.UserName).ToList();
                var responseData = new TWChatAppApiMaster.Socket.Message
                {
                    MsgType = (int)MessageType.TYPE_BLOCK,
                    Data = JsonConvert.SerializeObject(request)
                };
                await SendMessageToGroupUsersAsync(JsonConvert.SerializeObject(responseData), lstUser);
            }
            catch (Exception e)
            {

            }
            finally
            {
                _context.Dispose();
            }
        }

        /// <summary>
        /// Bỏ chặn tài khoản
        /// </summary>
        /// <param name="userSent"></param>
        /// <param name="request"></param>
        private async void processUnBlockMessage(string userSent, UnBlockMessage request)
        {
            var _context = ServiceExtension.GetDbContext();
            try
            {
                var roomInfo = await _context.Rooms
                    .AsNoTracking()
                    .Include(x => x.RoomMembers)
                    .Where(x => x.Type == 2)
                    .Where(x => x.Uuid == request.RoomUuid)
                    .Where(x => x.RoomMembers.Any(x => x.UserName == userSent && x.InRoom == 1))
                    .SingleOrDefaultAsync();

                var roomBlock = await _context.RoomBlock
                    .FirstOrDefaultAsync(x => x.UserName == request.UserName && x.RoomUuid == request.RoomUuid);

                var isAllow = await GroupService.CheckPermission(request.RoomUuid, userSent, edTypeGroupPermisson.BLOCK_MEMBER);// Kiểm tra có được gắn chức năng này không

                if (roomInfo == null
                    || (roomInfo.RoomMembers.Any(x => x.UserName == userSent && x.RoleId != 1) && !isAllow)
                    || roomBlock == null
                    || roomBlock.State == (int)edBlockState.UNBLOCK)
                {
                    var data1 = new TWChatAppApiMaster.Socket.Message
                    {
                        MsgType = (int)MessageType.TYPE_ERROR,
                        Data = EncodeBase64($"Đã có lỗi xảy ra. Bạn không thể bỏ chặn người dùng này")
                    };
                    await SendMessageAsync(userSent, JsonConvert.SerializeObject(data1));
                    return;
                }

                roomBlock.State = (int)edBlockState.UNBLOCK;
                _context.Update(roomBlock);
                await _context.SaveChangesAsync();

                //var lstUser = new List<string>() { userSent, request.UserName };
                var lstUser = roomInfo.RoomMembers.Where(x => x.InRoom == 1).Select(x => x.UserName).ToList();
                var responseData = new TWChatAppApiMaster.Socket.Message
                {
                    MsgType = (int)MessageType.TYPE_UNBLOCK,
                    Data = JsonConvert.SerializeObject(request)
                };
                await SendMessageToGroupUsersAsync(JsonConvert.SerializeObject(responseData), lstUser);
            }
            catch (Exception e)
            {
                var data1 = new TWChatAppApiMaster.Socket.Message
                {
                    MsgType = (int)MessageType.TYPE_ERROR,
                    Data = EncodeBase64($"Đã có lỗi xảy ra. {e.Message}")
                };
                await SendMessageAsync(userSent, JsonConvert.SerializeObject(data1));
            }
            finally
            {
                _context.Dispose();
            }
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
                    if (msg.MsgType == (int)MessageType.TYPE_CHAT)
                    {
                        ClientMessage clientMessage = TryDeserializeClientMessage(msg.Data);

                        if (clientMessage == null || string.IsNullOrEmpty(clientMessage.Receiver))
                        {
                            return;
                        }

                        processClientMessage(user, clientMessage);
                    }
                    else if (msg.MsgType == (int)MessageType.TYPE_TYPING)
                    {
                        TypingRequest typingMsg = TryDeserializeTypingMessage(msg.Data);

                        if (typingMsg == null)
                        {
                            return;
                        }

                        processTypingMessage(user, typingMsg);
                    }
                    else if (msg.MsgType == (int)MessageType.TYPE_READ)
                    {
                        ReadRequest readMsg = TryDeserializeReadMessage(msg.Data);

                        if (readMsg == null)
                        {
                            return;
                        }

                        processReadMessage(user, readMsg);
                    }
                    else if (msg.MsgType == (int)MessageType.TYPE_DELETE)
                    {
                        DeleteRequest delMsg = TryDeserializeDeleteMessage(msg.Data);

                        if (delMsg == null)
                        {
                            return;
                        }

                        processDeleteMessage(user, delMsg, type: 1);
                    }
                    else if (msg.MsgType == (int)MessageType.TYPE_DELETE_ONLY)
                    {
                        DeleteRequest delMsg = TryDeserializeDeleteMessage(msg.Data);

                        if (delMsg == null)
                        {
                            return;
                        }

                        processDeleteMessage(user, delMsg, type: 2);
                    }
                    else if (msg.MsgType == (int)MessageType.TYPE_PIN)
                    {
                        PinRequest pinMsg = TryDeserializePinMessage(msg.Data);

                        if (pinMsg == null)
                        {
                            return;
                        }

                        processPinMessage(user, pinMsg);
                    }
                    else if (msg.MsgType == (int)MessageType.TYPE_CHECK_ONLINE_STATE)
                    {
                        checkOnlineState(user, msg.Data);
                    }
                    else if (msg.MsgType == (int)MessageType.TYPE_FORWARD)
                    {
                        ForwardMessageRequest forwardMsg = TryDeserializeForwardMessage(msg.Data);

                        if (forwardMsg == null)
                        {
                            return;
                        }

                        processForwardMessage(user, forwardMsg);
                    }
                    else if (msg.MsgType == (int)MessageType.TYPE_EDIT)
                    {
                        EditMessageRequest editMsg = TryDeserializeEditMessage(msg.Data);

                        if (editMsg == null)
                        {
                            return;
                        }

                        processEditMessage(user, editMsg);
                    }
                    else if (msg.MsgType == (int)MessageType.TYPE_LIKE_MSG)
                    {
                        LikeMessage likeMsg = TryDeserializeLikeMessage(msg.Data);

                        if (likeMsg == null)
                        {
                            return;
                        }

                        processLikeMessage(user, likeMsg);
                    }
                    else if (msg.MsgType == (int)MessageType.TYPE_BAN)
                    {
                        BanMessage banMsg = TryDeserializeBanMessage(msg.Data);

                        if (banMsg == null)
                        {
                            return;
                        }

                        processBanMessage(user, banMsg);
                    }
                    else if (msg.MsgType == (int)MessageType.TYPE_UNBAN)
                    {
                        UnBanMessage unBanMsg = TryDeserializeUnBanMessage(msg.Data);

                        if (unBanMsg == null)
                        {
                            return;
                        }

                        processUnBanMessage(user, unBanMsg);
                    }
                    else if (msg.MsgType == (int)MessageType.TYPE_HIDE_MSG_USER)
                    {
                        HideMessageUser hideMessageUser = TryDeserializeHideMessageUser(msg.Data);

                        if (hideMessageUser == null)
                        {
                            return;
                        }

                        processHideMessageUser(user, hideMessageUser);
                    }
                    else if (msg.MsgType == (int)MessageType.TYPE_DELETE_MSG_USER)
                    {
                        DeleteMessageUser deleteMessageUser = TryDeserializeDeleteMessageUser(msg.Data);

                        if (deleteMessageUser == null)
                        {
                            return;
                        }

                        processDeleteMessageUser(user, deleteMessageUser);
                    }
                    else if (msg.MsgType == (int)MessageType.TYPE_BLOCK)
                    {
                        BlockMessage blockMessage = TryDeserializeBlockMessage(msg.Data);

                        if (blockMessage == null)
                        {
                            return;
                        }

                        processBlockMessage(user, blockMessage);
                    }
                    else if (msg.MsgType == (int)MessageType.TYPE_UNBLOCK)
                    {
                        UnBlockMessage unBlockMessage = TryDeserializeUnBlockMessage(msg.Data);

                        if (unBlockMessage == null)
                        {
                            return;
                        }

                        processUnBlockMessage(user, unBlockMessage);
                    }
                }

                var acc = _context.Account
                    .FirstOrDefault(x => x.UserName == user);

                if (acc != null)
                {
                    acc.LastSeen = DateTime.Now;
                    _context.Account.Update(acc);
                    _context.SaveChanges();
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

        private ClientMessage TryDeserializeClientMessage(string str)
        {
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<ClientMessage>(str);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private ForwardMessageRequest TryDeserializeForwardMessage(string str)
        {
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<ForwardMessageRequest>(str);
            }
            catch (Exception ex)
            {
                return null;
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

        private TypingRequest TryDeserializeTypingMessage(string str)
        {
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<TypingRequest>(str);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private ReadRequest TryDeserializeReadMessage(string str)
        {
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<ReadRequest>(str);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private DeleteRequest TryDeserializeDeleteMessage(string str)
        {
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<DeleteRequest>(str);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private PinRequest TryDeserializePinMessage(string str)
        {
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<PinRequest>(str);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private EditMessageRequest TryDeserializeEditMessage(string str)
        {
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<EditMessageRequest>(str);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private LikeMessage TryDeserializeLikeMessage(string str)
        {
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<LikeMessage>(str);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private UpSertMemGroupMessage TryDeserializeUpsertMessage(string str)
        {
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<UpSertMemGroupMessage>(str);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private BanMessage TryDeserializeBanMessage(string str)
        {
            try
            {
                using (JsonDocument doc = JsonDocument.Parse(str))
                {
                    JsonElement root = doc.RootElement;
                    var message = new BanMessage();
                    message.RoomUuid = root.GetProperty("RoomUuid").GetString();

                    //JsonElement dataElement = root.GetProperty("Data");
                    message.UserName = root.GetProperty("UserName").GetString();
                    message.User = root.GetProperty("User").ToString();

                    return message;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private UnBanMessage TryDeserializeUnBanMessage(string str)
        {
            try
            {
                using (JsonDocument doc = JsonDocument.Parse(str))
                {
                    JsonElement root = doc.RootElement;
                    var message = new UnBanMessage();
                    message.RoomUuid = root.GetProperty("RoomUuid").GetString();

                    //JsonElement dataElement = root.GetProperty("Data");
                    message.UserName = root.GetProperty("UserName").GetString();
                    message.User = root.GetProperty("User").ToString();

                    return message;
                }

                //return System.Text.Json.JsonSerializer.Deserialize<UnBanMessage>(str);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private HideMessageUser TryDeserializeHideMessageUser(string str)
        {
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<HideMessageUser>(str);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private DeleteMessageUser TryDeserializeDeleteMessageUser(string str)
        {
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<DeleteMessageUser>(str);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private BlockMessage TryDeserializeBlockMessage(string str)
        {
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<BlockMessage>(str);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private UnBlockMessage TryDeserializeUnBlockMessage(string str)
        {
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<UnBlockMessage>(str);
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

        public static async void HandleMessenge(MessageChatQueue msgQueue)
        {
            var _context = ServiceExtension.GetDbContext();

            try
            {
                //update database
                var msgGroup = _context.Rooms.Where(x => x.Uuid == msgQueue.MsgRoomUuid).SingleOrDefault();

                if (msgGroup != null)
                {
                    msgGroup.LastMessageUuid = msgQueue.Uuid;

                    var newMsgLine = new Messages
                    {
                        Uuid = msgQueue.Uuid,
                        Content = msgQueue.Content,
                        ContentType = msgQueue.ContentType,
                        RoomUuid = msgQueue.MsgRoomUuid,
                        ReplyMessageUuid = msgQueue.ReplyMsgUuid,
                        UserSent = msgQueue.UserSent,
                        Status = 1,
                        LanguageCode = msgQueue.CountryCode,
                        FileInfo = msgQueue.FileInfo,
                    };

                    msgGroup.Status = 1;
                    _context.Messages.Add(newMsgLine);
                    _context.Rooms.Update(msgGroup);

                    _context.SaveChanges();
                }

                //call firebase
                if (msgQueue.Content.Length > 2040)
                {
                    msgQueue.Content = msgQueue.Content.Substring(0, 2040);
                }

                if (
                    msgQueue.ListUsersOffline != null
                    && msgQueue.ListUsersOnline != null
                    && (msgQueue.ListUsersOnline.Count > 0 || msgQueue.ListUsersOffline.Count > 0)
                )
                {
                    //Gửi notify đến người dùng
                    var allAcc = msgQueue.ListUsersOffline.Concat(msgQueue.ListUsersOnline).ToList();
                    msgQueue.ServerMsg.Content = Base64Decode(msgQueue.ServerMsg.Content);
                    FirebaseCloudMessage.SendMulticastMessage(_context, allAcc, msgQueue.ServerMsg);
                }

                //TODO: Add thông tin người xem vào bảng msg_read
                ChatHandler.getInstance().processReadMessage(msgQueue.UserSent, new ReadRequest
                {
                    MsgUuid = msgQueue.Uuid,
                    RoomUuid = msgQueue.MsgRoomUuid
                });
            }
            catch (Exception e)
            {

                throw;
            }
            finally
            {
                _context.Dispose();
            }
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

    }
}
