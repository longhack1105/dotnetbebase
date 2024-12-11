using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Services.Aad;
using Microsoft.VisualStudio.Services.Users;
using System.Text;
using System.Threading.Tasks;
using TWChatAppApiMaster.Databases.ChatApp;

namespace ChatApp.Extensions
{
    public static class MessengeExtension
    {
        public static async Task<IQueryable<Rooms>> GetRoomAvailable(
            DBContext _context, 
            string? userName, 
            bool isRemoveRoomDelete = true, 
            string? roomUuid = null
        )
        {
            var rooms = _context.Rooms
                .AsNoTracking()
                //lấy nhóm chỉ định hoặc lấy hết
                .Where(x => String.IsNullOrEmpty(roomUuid) || x.Uuid == roomUuid)
                //lấy nhóm với user trong nhóm hoặc lấy hết
                .Where(x => String.IsNullOrEmpty(userName) || x.RoomMembers.Any(xx => xx.UserName == userName && xx.InRoom == 1))
                //bỏ nhóm bị xoá
                .Where(x => x.Status != 4)
                //nếu !isGetRoomDelete lọc bỏ nhóm bị xoá lịch sử (chỉ bỏ trong trường hợp chưa nhắn tin mới)
                .Where(x => !isRemoveRoomDelete || !(x.RoomDelete.Where(xx => xx.UserName == userName).Any()) || x.Messages
                            .Where(xx =>
                                xx.Id > (
                                        x.RoomDelete.Where(xx => xx.UserName == userName).Any()
                                        ? x.RoomDelete.Where(xx => xx.UserName == userName).Max(xx => xx.LastMessageId)
                                        : 0
                                    )
                            )
                            .Any()
                )
                ;

            return rooms;
        }
        public static async Task<Messages?> GetLastMessengeInRoom(DBContext _context, string roomUuid, string userName)
        {
            var messagesAvailable = GetMessagesAvailable(_context.Messages, userName);
            var LastMessage = messagesAvailable
                .AsNoTracking()
                .Include(x => x.UserSentNavigation)
                .Include(x => x.MessageRead)
                .Include(x => x.MessageLike)
                .Include(x => x.MessageDelete)
                .Where(x => x.RoomUuid == roomUuid)
                .OrderByDescending(x => x.Id)
                .FirstOrDefault();

            return LastMessage;
        }
        public static async Task<int> GetUnreadTotal(DBContext _context, string userName)
        {
            var totalUnread = 0;

            //lấy room đang hoạt động
            var roomAvailable = await GetRoomAvailable(_context, userName);

            //ds room
            totalUnread = roomAvailable
                .Include(x => x.Messages)
                    .ThenInclude(x => x.MessageDelete.Where(md => md.UserName == userName))
                .Include(x => x.MessageRead)
                .Include(x => x.RoomDelete)
                .Where(x => x.Messages.Any()) //Có ít nhất 1 tin nhắn
                .Select(x => new
                {
                    Uuid = x.Uuid,
                    Messages = x.Messages,
                    MessageReadTo = x.MessageRead.Where(xx => xx.UserName == userName).Any() ? x.MessageRead.Where(xx => xx.UserName == userName).Max(xx => xx.LastMessageId) : 0,
                    MessageDeleteTo = x.RoomDelete.Where(xx => xx.UserName == userName).Any() ? x.RoomDelete.Where(xx => xx.UserName == userName).Max(xx => xx.LastMessageId) : 0,
                })
                .Select(x => new
                {
                    UnreadCount = x.Messages.Count(xx =>
                        xx.Status != 4 && //bỏ tin nhắn trạng thái xoá 
                        !xx.MessageDelete.Where(xx => xx.UserName == userName).Any() //bỏ tin nhắn bị xoá 
                        && xx.Id > x.MessageReadTo //bỏ tin nhắn đã đọc
                        && xx.Id > x.MessageDeleteTo //bỏ tin nhắn xoá tới
                    ),
                    //UnreadCount = GetUnreadCount(_context, userName, null, x.Messages, x.MessageReadTo, x.MessageDeleteTo),
                })
                .Sum(x => x.UnreadCount);

            ////tính tin chưa đọc
            //foreach (var room in rooms)
            //{
            //    totalUnread += await GetUnreadCount(_context, userName, room.Uuid, room.Messages, room.MessageReadTo, room.MessageDeleteTo);
            //}

            return totalUnread;
        }
        public static int GetUnreadCount(
            DBContext _context,
            string userName,
            string roomUuid,
            ICollection<Messages> messages,
            long? messageReadTo = null,
            long? messageDeleteTo = null
        )
        {
            //lấy tin nhắn đọc tới
            if (messageReadTo == null)
            {
                var queryReadTo = _context.MessageRead
                    .AsNoTracking()
                    .Where(x2 => x2.RoomUuid == roomUuid && x2.UserName == userName);
                messageReadTo = queryReadTo.Any() ? queryReadTo.Max(x => x.LastMessageId) : 0;
            }

            //lấy tin nhắn xoá tới
            if (messageDeleteTo == null)
            {
                var queryDeleteTo = _context.RoomDelete
                    .AsNoTracking()
                    .Where(x => x.RoomUuid == roomUuid && x.UserName == userName);

                messageDeleteTo = queryDeleteTo.Any() ? queryDeleteTo.Max(x => x.LastMessageId ?? 0) : 0;
            }

            //đếm tin nhắn chưa đọc
            var unreadCount = messages
                .Count(x =>
                    x.Status != 4 && //bỏ tin nhắn trạng thái xoá 
                    !x.MessageDelete.Where(xx => xx.UserName == userName).Any() //bỏ tin nhắn bị xoá 
                    && x.Id > messageReadTo //bỏ tin nhắn đã đọc
                    && x.Id > messageDeleteTo //bỏ tin nhắn xoá tới
                );

            return unreadCount;
        }
        public static int GetUnreadCount(
            DBContext _context,
            string userName,
            string roomUuid,
            ICollection<Messages> messages,
            ICollection<MessageRead> messageReadTo = null,
            ICollection<RoomDelete> messageDeleteTo = null
        )
        {
            long messageReadToId = 0;
            long messageDeleteToId = 0;
            //lấy tin nhắn đọc tới
            if (messageReadTo == null)
            {
                var queryReadTo = _context.MessageRead
                    .AsNoTracking()
                    .Where(x2 => x2.RoomUuid == roomUuid && x2.UserName == userName);
                messageReadToId = queryReadTo.Any() ? queryReadTo.Max(x => x.LastMessageId) : 0;
            }
            else
            {
                var messageReadTo_ = messageReadTo
                    .Where(xx => xx.UserName == userName);
                messageReadToId = messageReadTo_.Any() ? messageReadTo_.Max(x => x.LastMessageId) : 0;
            }

            //lấy tin nhắn xoá tới
            if (messageDeleteTo == null)
            {
                var queryDeleteTo = _context.RoomDelete
                    .AsNoTracking()
                    .Where(x => x.RoomUuid == roomUuid && x.UserName == userName);

                messageDeleteToId = queryDeleteTo.Any() ? queryDeleteTo.Max(x => x.LastMessageId ?? 0) : 0;
            }
            else
            {
                var messageDeleteTo_ = messageDeleteTo
                    .Where(xx => xx.UserName == userName);
                messageDeleteToId = messageDeleteTo_.Any() ? messageDeleteTo_.Max(x => x.LastMessageId ?? 0) : 0;
            }

            //đếm tin nhắn chưa đọc
            var unreadCount = GetUnreadCount(_context, userName, roomUuid, messages, messageReadToId, messageDeleteToId);

            return unreadCount;
        }
        //public static ICollection<Messages> GetUnreadCountQuery(ICollection<Messages> messages, string userName, long? messageReadTo = null, long? messageDeleteTo = null)
        //{
        //    //đếm tin nhắn chưa đọc
        //    ICollection<Messages> unreadCount = messages
        //        .Count(x =>
        //            x.Status != 4 && //bỏ tin nhắn trạng thái xoá 
        //            !x.MessageDelete.Where(xx => xx.UserName == userName).Any() //bỏ tin nhắn bị xoá 
        //            && x.Id > messageReadTo //bỏ tin nhắn đã đọc
        //            && x.Id > messageDeleteTo //bỏ tin nhắn xoá tới
        //        );

        //    return unreadCount;
        //}
        public static IQueryable<Messages> GetMessagesAvailable(IQueryable<Messages> messages, string accUser)
        {
            var results = messages
                .Where(x => !x.MessageDelete.Any(x => x.UserName == accUser)) //bỏ tin nhắn xoá với tài khoản
                .Where(x => !(x.Status == 3 && x.RoomUu.RoomMembers.Where(xx => xx.RoleId == 3 && xx.UserName == accUser).Any())) //bỏ tin nhắn ẩn với user
                .Where(x => x.Status != 4); //bỏ tin nhắn đã bị xoá

            return results;
        }
        public static IQueryable<Messages> GetMessagesAvailableByRoom(IQueryable<Messages> messages, string accUser, string roomUuid)
        {
            var messagesAvailable = GetMessagesAvailable(messages, accUser); //bỏ tin nhắn đã bị xoá

            var messagesAvailableByRoom_ = messagesAvailable
                .Include(x => x.RoomDelete)
                .Where(x => x.RoomUuid == roomUuid) //lấy tin nhắn với roomUuid
            ;

            var deleteTo = messagesAvailable.Max(x => x.RoomDelete
                                                        .Where(xx => xx.UserName == accUser && xx.RoomUuid == roomUuid)
                                                        .Select(xx => xx.LastMessageId)
                                                        .Max()
            ) ?? 0;

            var messagesAvailableByRoom = messagesAvailableByRoom_
                .Where(x => x.Id > deleteTo) //bỏ tin nhắn room bị xoá
            ;

            return messagesAvailableByRoom;
        }

        public static string EncodeBase64(string value)
        {
            var valueBytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(valueBytes);
        }
        public static string DecodeBase64(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
