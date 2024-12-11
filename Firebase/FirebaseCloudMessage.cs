using TWChatAppApiMaster.Databases.ChatApp;
using ChatApp.Models.DataInfo;
using ChatApp.Socket;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Notification = FirebaseAdmin.Messaging.Notification;
using Newtonsoft.Json.Linq;
using ChatApp.Extensions;

namespace ChatApp.Firebase
{
    public static class FirebaseCloudMessage
    {
        private static string channelId = "chat_app";
        public static async Task SendMulticastMessage(this DBContext context, List<string> lstSendToUsers, MessageLineDTO dto)
        {
            foreach (var userName in lstSendToUsers)
            {
                var account = context.Account.AsNoTracking()
                    .Where(x => x.UserName == userName)
                    .FirstOrDefault();

                var fcmTokens = context.Session.AsNoTracking()
                    .Where(x => x.UserName == userName)
                    .Where(x => x.Status == 0)
                    .Where (x => x.TimeExpired > DateTime.Now)
                    .Select(x => x.FcmToken)
                    .Distinct()
                    .ToList();

                if (account != null && fcmTokens.Any())
                {
                    if (account.ReceiveNotifyStatus == 0) return;

                    string content = dto.Content;
                    if (dto.ContentType == 3)
                        content = "send media";
                    else if (dto.ContentType == 4)
                        content = "send file";
                    else if (dto.ContentType == 5)
                        content = "send audio";
                    else if (dto.ContentType == 6)
                        content = "send sticker";
                    else if (dto.ContentType == 8)
                        content = "send GIF";

                    //var message = new MulticastMessage()
                    //{
                    //    //Token = account.FcmToken,
                    //    Tokens = fcmTokens,
                    //    Notification = new Notification()
                    //    {
                    //        Title = Base64Decode(dto.FullName),
                    //        Body = content
                    //    },
                    //    Data = new Dictionary<string, string?>()
                    //    {
                    //        {"type", $"{dto.ContentType}"},
                    //        {"groupUuid", dto.MsgRoomUuid},
                    //        {"lineUuid", dto.Uuid}
                    //    },
                    //    Android = new()
                    //    {
                    //        Priority = Priority.High,
                    //        Notification = new()
                    //        {
                    //            ChannelId = channelId,
                    //        }
                    //    },
                    //    Apns = new ApnsConfig
                    //    {
                    //        Aps = new Aps
                    //        {
                    //            ContentAvailable = true,
                    //            MutableContent = true,
                    //        }
                    //    },
                    //};
                    List<Message> message = new List<Message>();
                    foreach (var token in fcmTokens)
                    {
                        message.Add(new Message
                        {
                            Token = token,
                            Data = new Dictionary<string, string?>()
                            {
                                {"notificationTitle", MessengeExtension.DecodeBase64(dto.FullName)},
                                {"notificationBody", content},
                                {"type", $"{dto.Type}"},
                                {"contentType", $"{dto.ContentType}"},
                                {"groupUuid", dto.MsgRoomUuid},
                                {"lineUuid", dto.Uuid},
                                {"roomName", dto.RoomName},
                                {"ownerUuid", dto.OwnerUuid},
                                {"avatar", dto.Avatar},
                                {"roomAvatar", dto.RoomAvatar},
                                {"userSent", dto.UserSent},
                            },
                            Android = new()
                            {
                                Priority = Priority.High,
                                //Notification = new()
                                //{
                                //    ChannelId = channelId,
                                //}
                            },
                            //Apns = new ApnsConfig
                            //{
                            //    Aps = new Aps
                            //    {
                            //        ContentAvailable = true,
                            //        MutableContent = true,
                            //    },
                            //},
                            Apns = new ApnsConfig
                            {
                                Aps = new Aps
                                {
                                    Alert = new ApsAlert
                                    {
                                        Title = dto.Type == 1 ? MessengeExtension.DecodeBase64(dto.FullName) : MessengeExtension.DecodeBase64(dto.RoomName),
                                        Body = (dto.Type == 2 ? MessengeExtension.DecodeBase64(dto.FullName) + ": " : "") + content,
                                    },
                                    Sound = "default" // Âm thanh cho iOS
                                }
                            }
                        });
                    }

                    if (message == null || message.Count <= 0)
                    {
                        return;
                    }

                    foreach (var messengeItem in message)
                    {
                        try
                        {
                            FirebaseMessaging.DefaultInstance.SendAsync(messengeItem);
                        }
                        catch (Exception er)
                        {

                        }

                    }

                    //await FirebaseMessaging.DefaultInstance.SendEachAsync(message);

                }
            }
        }
    }
}
