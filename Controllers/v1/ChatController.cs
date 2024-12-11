using ApiBuyerMorgan.Extensions;
using ChatApp.Enum;
using ChatApp.Extensions;
using ChatApp.Models.DataInfo;
using ChatApp.Models.Request;
using ChatApp.Models.Response;
using ChatApp.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using TWChatAppApiMaster.Databases.ChatApp;
using TWChatAppApiMaster.Models.DataInfo;
using static ChatApp.Enums.EnumDatabase;
using Microsoft.EntityFrameworkCore.Query;
using System;

namespace ChatApp.Controllers.v1
{
    [Authorize]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class ChatController : ControllerBase
    {
        private readonly DBContext _context;
        private readonly ILogger<ChatController> _logger;
        public ChatController(DBContext context, ILogger<ChatController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Tạo room chat
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("create-message-room")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessage<string>), description: "Create Messages Room Response")]
        public async Task<IActionResult> CreateMessageRoom([FromBody] CreateMessageRoomRequest request)
        {
            var accUser = User.GetUserName();
            var accUuid = User.GetAccountUuid();
            var accFullName = User.GetFullName();

            var response = new BaseResponseMessage<string>();

            try
            {
                // Chat 1-1
                if (request.Type == 1)
                {
                    var accountDb = await _context.Account.FirstOrDefaultAsync(x => x.Uuid == request.OwnerUuid);
                    if (accountDb is null)
                    {
                        response.error.SetErrorCode(ErrorCode.NOT_FOUND);
                        return new OkObjectResult(response);
                    }

                    var roomDb = await _context.Rooms
                        .AsNoTracking()
                        .Where(x => x.Type == 1)
                        .Where(x => x.RoomMembers.Count(x => x.UserName == accUser || x.UserName == accountDb.UserName) == 2)
                        .FirstOrDefaultAsync();

                    if (roomDb == null)
                    {
                        var friendDb = await _context.Friends
                            .AsNoTracking()
                            .Where(x => x.Status == 3)
                            .Where(x => x.UserSent == accUser && x.UserReceiver == accountDb.UserName
                                || x.UserSent == accountDb.UserName && x.UserReceiver == accUser)
                            .FirstOrDefaultAsync();

                        roomDb = new Rooms()
                        {
                            Uuid = Guid.NewGuid().ToString(),
                            Creater = accountDb.UserName,
                            IsAllow = (ulong)(friendDb != null ? 1 : 0),
                            RoomName = $"{accFullName} - {accountDb.FullName}",
                            Status = 1,
                            Type = 1,
                        };
                        await _context.Rooms.AddAsync(roomDb);

                        await _context.RoomMembers.AddAsync(new RoomMembers()
                        {
                            UserName = accUser,
                            RoomUuid = roomDb.Uuid,
                            RoleId = 3,
                        });

                        await _context.RoomMembers.AddAsync(new RoomMembers()
                        {
                            UserName = accountDb.UserName,
                            RoomUuid = roomDb.Uuid,
                            RoleId = 3,
                        });

                        await _context.SaveChangesAsync();
                    }

                    response.Data = roomDb.Uuid;
                }
                // Chat nhóm: đã có api tạo roomchat nhóm
                else
                {
                    var roomDb = await _context.Rooms
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Uuid == request.OwnerUuid);

                    if (roomDb is null)
                    {
                        response.error.SetErrorCode(ErrorCode.NOT_FOUND);
                        return new OkObjectResult(response);
                    }

                    response.Data = roomDb.Uuid;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// Danh sách room chát
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("message-room")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessageItem<MessagesGroupDTO>), description: "Load Messages Room Response")]
        public async Task<IActionResult> LoadMessageRoom([FromBody] GetMessageRoomRequest request)
        {
            var accUser = User.GetUserName();
            var response = new ResMessagesGroupDTO();

            try
            {
                // Lấy danh sách thành viên người dùng có vai trò chủ/phó phòng
                var checkRoleAdmin = await _context.RoomMembers
                    .AsNoTracking()
                    .Where(x => x.UserName == accUser && x.RoleId != 3 && x.InRoom == 1)
                    .Select(x => x.RoomUuid)
                    .ToListAsync();

                // Truy vấn rooms cơ bản
                var roomAvailable = await MessengeExtension.GetRoomAvailable(_context, accUser, string.IsNullOrEmpty(request.Keyword));

                var data_ = await roomAvailable
                    .AsSplitQuery()
                    .Include(x => x.RoomMembers)
                        .ThenInclude(rm => rm.UserNameNavigation)
                    .Include(x => x.RoomPin)
                    .Include(x => x.Messages)
                        .ThenInclude(x => x.MessageDelete)
                    //lọc theo type
                    .Where(x => !(request.Type == 1 || request.Type == 2) || x.Type == request.Type)
                    .Where(x => string.IsNullOrEmpty(request.Keyword)
                        || EF.Functions.Like(x.Type == 2 ? x.RoomName : x.RoomMembers.First(rm => rm.UserName != accUser).UserNameNavigation.FullName, $"%{request.Keyword}%"))
                    //xắp xếp theo time pin
                    .OrderByDescending(x => x.RoomPin.Any(rp => rp.UserName == accUser && rp.State == 1)
                        ? x.RoomPin.First(rp => rp.UserName == accUser && rp.State == 1).TimePin
                        : DateTime.MinValue)
                        //xắp xếp theo thời gian tạo
                        .ThenByDescending(x => x.Messages.Any(m => m.Status != 4 && (checkRoleAdmin.Contains(m.RoomUuid) || m.Status != 3) && !m.MessageDelete.Any(md => md.UserName == accUser))
                            ? x.Messages.Where(m => m.Status != 4 && (checkRoleAdmin.Contains(m.RoomUuid) || m.Status != 3) && !m.MessageDelete.Any(md => md.UserName == accUser)).OrderByDescending(m => m.Id).First().TimeCreated
                            : (x.Type == 2
                                ? x.TimeCreated
                                : DateTime.MinValue))
                    .Select(x => x.Id)
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                var data = await _context.Rooms
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Where(x => data_.Contains(x.Id))
                    .Include(x => x.CreaterNavigation)
                    .Include(x => x.Messages)
                        .ThenInclude(x => x.MessageDelete)
                    .Include(x => x.Messages)
                        .ThenInclude(x => x.UserSentNavigation)
                    .Include(x => x.Messages)
                        .ThenInclude(x => x.MessageLike)
                    .Include(x => x.Messages)
                        .ThenInclude(x => x.MessageRead)
                    .Include(x => x.RoomMembers)
                        .ThenInclude(rm => rm.UserNameNavigation)
                    .Include(x => x.RoomPin)
                    .Include(x => x.MessageRead)
                    .Include(x => x.RoomDelete)
                    .ToListAsync();

                response.Items.AddRange(data.Select(x =>
                {
                    var dto = new MessagesGroupDTO
                    {
                        Id = x.Id,
                        Uuid = x.Uuid,
                        Type = x.Type,
                        LastUpdated = x.LastUpdated,
                        TimeCreated = x.TimeCreated,
                        Pinned = x.RoomPin.Any(rp => rp.UserName == accUser && rp.State == 1),
                        CreatorFullName = x.CreaterNavigation?.FullName ?? "",
                    };

                    var LastMessage = x.Messages
                        .Where(m => m.Status != 4 && (checkRoleAdmin.Contains(m.RoomUuid) || m.Status != 3) && !m.MessageDelete.Any(md => md.UserName == accUser))
                        .OrderByDescending(m => m.Id)
                        .FirstOrDefault();

                    if (LastMessage != null)
                    {
                        dto.Content = LastMessage.Content;
                        dto.ContentType = LastMessage.ContentType;
                        dto.ForwardFrom = LastMessage.ForwardFrom;
                        dto.FullName = LastMessage.UserSentNavigation.FullName;
                        dto.LastMsgLineUuid = LastMessage.Uuid;
                        dto.ReadCounter = LastMessage.MessageRead.Count(mr => mr.UserName != accUser);
                        dto.LikeCount = LastMessage.MessageLike.Count();
                        dto.UserSent = LastMessage.UserSent;
                        dto.UserSentIsBlock = LastMessage.UserSentNavigation.RoomBlock.SingleOrDefault()?.State ?? 0;
                        dto.Status = LastMessage.Status;
                        dto.IsDeleteWithMe = LastMessage.MessageDelete.Any(x2 => x2.UserName == accUser);
                        dto.UnreadCount = MessengeExtension.GetUnreadCount(
                            _context,
                            accUser,
                            x.Uuid,
                            x.Messages,
                            x.MessageRead,
                            x.RoomDelete
                        );
                        dto.LastUpdated = LastMessage.LastEdited;
                        dto.FileInfo = LastMessage.FileInfo != null ? EncodeBase64(LastMessage.FileInfo) : null;
                    }
                    else if (x.Type == 2)
                    {
                        dto.UserSent = "admin";
                        dto.Content = x.CreaterNavigation.FullName ?? x.CreaterNavigation.UserName;
                        dto.ContentType = 1;
                        dto.UnreadCount = 0;
                        dto.LastUpdated = x.TimeCreated;
                    }

                    if (x.Type == 1)
                    {
                        var member = x.RoomMembers.FirstOrDefault(rm => rm.UserName != accUser)?.UserNameNavigation;
                        dto.Avatar = member?.Avatar;
                        dto.OwnerUuid = x.CreaterNavigation.Uuid;
                        dto.PartnerUuid = member?.Uuid;
                        dto.OwnerName = x.CreaterNavigation.FullName ?? x.CreaterNavigation.UserName;
                        dto.ShowName = member?.FullName ?? member?.UserName;
                        dto.ShowUuid = member?.Uuid;
                    }
                    else
                    {
                        dto.Avatar = x.Avatar;
                        dto.OwnerUuid = x.Uuid;
                        dto.OwnerName = x.RoomName;
                        dto.ShowName = x.RoomName;
                        dto.ShowUuid = x.Uuid;
                    }
                    return dto;
                }));

                response.Items = response.Items.OrderBy(x => data_.IndexOf((long)x.Id)).ToList();

                //response.UnreadTotal = await MessengeExtension.GetUnreadTotal(_context, accUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading message rooms.");
                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// Lấy UnreadTotal
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("get-unread-total")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessage<long>), description: "Lấy UnreadTotal")]
        public async Task<IActionResult> GetUnreadTotal()
        {
            var accUser = User.GetUserName();
            var response = new BaseResponseMessage<long>();

            try
            {
                response.Data = await MessengeExtension.GetUnreadTotal(_context, accUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading message rooms.");
                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// Danh sách tin nhắn
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("message-line")]
        [SwaggerResponse(statusCode: 200, type: typeof(MessageLineResponseMessage<MessageLineDTO>), description: "Load Messages In Line Response")]
        public async Task<IActionResult> LoadMessageLine([FromBody] LoadMessageRequest request)
        {
            var accUser = User.GetUserName();
            var response = new MessageLineResponseMessage<MessageLineDTO>();

            try
            {
                var results = await _context.Messages
                    .AsNoTracking()
                    .Where(x => x.RoomUuid == request.MsgGroupUuid)
                    .Where(x => x.Status != 4)
                    //.Where(x => !x.MessageDelete.Any(x => x.UserName == accUser))
                    .Where(x => x.Status != 3
                        || x.RoomUu.RoomMembers.Any(x => x.UserName == accUser && x.RoleId != 3))
                    .Where(x => x.RoomUu.RoomMembers.Any(x => x.UserName == accUser && x.InRoom == 1))
                    .Where(x => !x.RoomUu.RoomDelete.Any(x => x.UserName == accUser)
                        || x.RoomUu.RoomDelete.OrderByDescending(x => x.Id).First(x => x.UserName == accUser).LastMessageId < x.Id)
                    .Where(x => string.IsNullOrEmpty(request.Keyword) || EF.Functions.Like(x.Content, $"%{request.Keyword}%"))
                    .Where(x => string.IsNullOrEmpty(request.Keyword) ||
                        (
                            x.ContentType == (int)edContentType.TEXT
                            || x.ContentType == (int)edContentType.LINK
                        )
                    //|| EF.Functions.Like(x.FileInfo ?? "", !string.IsNullOrEmpty(request.Keyword) ? $"%\"fileName\":\"%{request.Keyword}%\"%" : $"%{request.Keyword}%") // là file thì check file name
                    )
                    .Where(x => !(x.MessageDelete != null ? x.MessageDelete.Any(x2 => x2.UserName == accUser) : false)) // bỏ tin nhắn xoá đối với tôi
                    .OrderByDescending(x => x.Id)
                    .Select(x => new MessageLineDTO()
                    {
                        Uuid = x.Uuid,
                        MsgRoomUuid = x.RoomUuid,
                        UserSent = x.UserSent,
                        IsBan = x.RoomUu.RoomBan.Any(y => y.UserName == x.UserSent && y.State == 1),
                        IsBlock = x.RoomUu.RoomBlock.Any(y => y.UserName == x.UserSent && y.State == 1),
                        IsDeleteWithMe = x.MessageDelete != null ? x.MessageDelete.Any(x2 => x2.UserName == accUser) : false,
                        FullName = x.UserSentNavigation.FullName ?? "",
                        Avatar = x.UserSentNavigation.Avatar ?? "",
                        Content = x.Content,
                        ContentType = x.ContentType,
                        FileInfo = x.FileInfo != null ? EncodeBase64(x.FileInfo) : null,
                        LastEdited = x.LastEdited,
                        CountryCode = x.LanguageCode,
                        ForwardFrom = x.ForwardFrom,
                        LikeCount = x.MessageLike.Count(),
                        Status = x.Status,
                        TimeCreated = x.TimeCreated,

                        ReplyMsgUuid = x.ReplyMessageUuid,
                        ReplyMsgUu = x.ReplyMessageUu != null
                            ? new MessageLineDTO
                            {
                                Uuid = x.ReplyMessageUu.Uuid,
                                UserSent = x.ReplyMessageUu.UserSent,
                                Content = x.ReplyMessageUu.Content,
                                ContentType = x.ReplyMessageUu.ContentType,
                                MsgRoomUuid = x.ReplyMessageUu.RoomUuid,
                                TimeCreated = x.ReplyMessageUu.TimeCreated,
                                Status = x.ReplyMessageUu.Status,
                                IsDeleteWithMe = x.ReplyMessageUu.MessageDelete != null ? x.ReplyMessageUu.MessageDelete.Any(x2 => x2.UserName == accUser) : false,
                                ReplyMsgUuid = x.ReplyMessageUu.ReplyMessageUuid,
                                LastEdited = x.ReplyMessageUu.LastEdited,
                                CountryCode = x.LanguageCode,
                                FullName = x.ReplyMessageUu.UserSentNavigation.FullName ?? "",
                                Avatar = x.ReplyMessageUu.UserSentNavigation.Avatar ?? "",
                                FileInfo = x.ReplyMessageUu.FileInfo != null ? EncodeBase64(x.ReplyMessageUu.FileInfo) : null,
                            } : null,
                        EmojiList = x.MessageLike.Where(x => x.Status == 1)
                            .Select(x => new ReactedDTO
                            {
                                Type = x.Type,
                                Uuid = x.UserNameNavigation.Uuid,
                                UserName = x.UserName,
                                FullName = x.UserNameNavigation.FullName ?? x.UserName,
                                Avatar = x.UserNameNavigation.Avatar,
                                TimeCreated = x.TimeCreated,
                            })
                            .ToList(),
                    })
                    .TakePage(request.Page, request.PageSize);

                foreach (var item in results.Where(x => x.ContentType == 4))
                {
                    var file = item.Content.Substring(2, item.Content.Length - 4);
                    var fileName = _context.FilesInfo.Where(x => x.Path == file).Select(x => x.FileName).SingleOrDefault();
                    item.MediaName = fileName ?? "";
                }

                response.Items = results;

                // check người gửi xem có bị ban không
                response.IsBan = _context.RoomBan
                    .AsNoTracking()
                    .Where(x => x.RoomUuid == request.MsgGroupUuid && x.UserName == accUser)
                    .Where(x => x.State == 1)
                    .Any();

                // lấy tin nhắn đã đọc cuối
                var LastMsgRead = _context.MessageRead
                    .AsNoTracking()
                    .Include(x => x.LastMessage)
                    .Where(x => x.RoomUuid == request.MsgGroupUuid && x.UserName != accUser)
                    .Select(x => new
                    {
                        LastMessageId = x.LastMessageId,
                        LastMessage = x.LastMessage,
                    })
                    .OrderByDescending(x => x.LastMessageId)
                    .FirstOrDefault();

                if (LastMsgRead != null)
                    response.LastMsgRead = LastMsgRead.LastMessage.Uuid;

                // lấy tin nhắn đã đọc cuối bới người gọi api
                var LastMsgReadByMe = _context.MessageRead
                    .AsNoTracking()
                    .Include(x => x.LastMessage)
                    .Where(x => x.RoomUuid == request.MsgGroupUuid && x.UserName == accUser)
                    .Select(x => new
                    {
                        LastMessageId = x.LastMessageId,
                        LastMessage = x.LastMessage,
                    })
                    .OrderByDescending(x => x.LastMessageId)
                    .FirstOrDefault();

                if (LastMsgReadByMe != null)
                    response.LastMsgReadByMe = LastMsgReadByMe.LastMessage.Uuid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
                response.error.Message = ex.Message;
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// Danh sách tin nhắn ghim
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("list-messageline-pinned")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessageItem<MessageLineDTO>), description: "Search Messages In Line Response")]
        public async Task<IActionResult> GetMsgLinePinned([FromBody] UuidRequest request)
        {
            var accUser = User.GetUserName();

            var response = new BaseResponseMessageItem<MessageLineDTO>();

            try
            {
                var lstMsg = await _context.Messages
                    .AsNoTracking()
                    .Where(x => x.RoomUuid == request.Uuid && x.Status != 4)
                    .Where(x => x.MessagePin.Any(x => x.State == 1))
                    .Where(x => !x.MessageDelete.Any(xx => xx.UserName == accUser))
                    .Where(x => x.Status != 4)
                    .Where(x => x.Status != 3
                        || x.RoomUu.RoomMembers.Any(x => x.UserName == accUser && x.RoleId != 3))
                    .Where(x => !x.RoomUu.RoomDelete.Any(x => x.UserName == accUser)
                        || x.RoomUu.RoomDelete.OrderByDescending(x => x.Id).First(x => x.UserName == accUser).LastMessageId < x.Id)
                    .OrderByDescending(x => x.MessagePin.OrderByDescending(x2 => x2.TimePin).Where(x2 => x2.State == 1).Select(x2 => x2.TimePin).FirstOrDefault())
                    .Select(x => new MessageLineDTO
                    {
                        Uuid = x.Uuid,
                        MsgRoomUuid = x.RoomUuid,
                        ReplyMsgUuid = x.ReplyMessageUuid,

                        UserSent = x.UserSent,
                        FullName = x.UserSentNavigation.FullName ?? "",
                        Avatar = x.UserSentNavigation.Avatar ?? "",

                        Content = x.Content,
                        ContentType = x.ContentType,
                        LastEdited = x.LastEdited,
                        TimeCreated = x.TimeCreated,
                        Status = x.Status,
                        CountryCode = x.LanguageCode,
                        FileInfo = x.FileInfo != null ? EncodeBase64(x.FileInfo) : null,
                    })
                    .ToListAsync();

                response.Items = lstMsg;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
                response.error.Message = ex.Message;
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// Xoá room
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpDelete("delete-message-room")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponse), description: "Delete Messages Room Response")]
        public async Task<IActionResult> DeleteMessageGroup([FromBody] UuidRequest request)
        {
            var accUser = User.GetUserName();
            var response = new BaseResponse();

            if (!string.IsNullOrEmpty(request.Uuid))
            {
                try
                {
                    var roomDb = await _context.Rooms
                        .Include(x => x.Messages)
                        .Include(x => x.RoomDelete.Where(x => x.UserName == accUser))
                        .Where(x => x.Uuid == request.Uuid)
                        .Where(x => x.RoomMembers.Any(x => x.UserName == accUser))
                        .FirstOrDefaultAsync();

                    if (roomDb == null)
                    {
                        response.error.SetErrorCode(ErrorCode.INVALID_PARAM);
                        return new OkObjectResult(response);
                    }

                    await _context.RoomDelete.AddAsync(new RoomDelete
                    {
                        RoomUuid = roomDb.Uuid,
                        UserName = accUser,
                        LastMessageId = roomDb.LastMessageUu?.Id,
                    });

                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, ex);
                    response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
                    response.error.Message = ex.Message;
                }
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// Xoá lịch sử tin nhắn
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpDelete("delete-history-message-room")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessage<object>), description: "Delete Messages Room Response")]
        public async Task<IActionResult> DeleteHistoreMessageGroup([FromBody] UuidRequest request)
        {
            var accUser = User.GetUserName();
            var response = new BaseResponseMessage<object>();

            if (!string.IsNullOrEmpty(request.Uuid))
            {
                try
                {
                    var roomDb = await _context.Rooms
                        .AsNoTracking()
                        .Include(x => x.Messages.Where(x => !x.MessageDelete.Any(x => x.UserName == accUser)))
                        .Where(x => x.Uuid == request.Uuid)
                        .Where(x => x.RoomMembers.Any(x => x.UserName == accUser && x.InRoom == 1))
                        .FirstOrDefaultAsync();

                    if (roomDb == null)
                    {
                        response.error.SetErrorCode(ErrorCode.INVALID_PARAM);
                        return new OkObjectResult(response);
                    }

                    var newDeleteMessages = roomDb.Messages
                        .Select(x => new MessageDelete
                        {
                            MessageUuid = x.Uuid,
                            UserName = accUser,
                        })
                        .ToList();

                    await _context.MessageDelete.AddRangeAsync(newDeleteMessages);
                    await _context.SaveChangesAsync();

                    response.Data = null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, ex);

                    response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
                    response.error.Message = ex.Message;
                }
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// Ghim/bỏ ghim room chát
        /// </summary>
        /// <param name="request"></param>
        /// <remarks>
        ///     - State:
        ///         + 0: bỏ ghim
        ///         + 1: ghim
        /// </remarks>
        /// <returns></returns>
        [HttpPost("pin-room")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessageItem<string>), description: "Pin Room Response")]
        public async Task<IActionResult> PinMessage([FromBody] PinGroupRequest request)
        {
            var accUser = User.GetUserName();
            var response = new BaseResponseMessage<string>();

            if (!string.IsNullOrEmpty(request.RoomUuid))
            {
                try
                {
                    var msg = await _context.RoomPin.Where(x => x.RoomUuid == request.RoomUuid && x.UserName == accUser).FirstOrDefaultAsync();
                    if (msg != null)
                    {
                        msg.State = (sbyte)request.State;
                        _context.SaveChanges();
                    }
                    else
                    {
                        await _context.RoomPin.AddAsync(new RoomPin
                        {
                            RoomUuid = request.RoomUuid,
                            UserName = accUser,
                            State = 1,
                        });
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, ex);
                    response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
                }
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// Số lượt đã đọc tin nhắn
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("check-message-read-state")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessage<int>), description: "Check Read State Message Response")]
        public async Task<IActionResult> CheckReadState([FromBody] UuidRequest request)
        {
            var accUser = User.GetUserName();
            var response = new BaseResponseMessage<int>();
            int count = 0;

            try
            {
                var messageDb = await _context.Messages
                    .AsNoTracking()
                    .Where(x => x.Uuid == request.Uuid)
                    .FirstOrDefaultAsync();

                if (messageDb != null)
                {
                    count = await _context.MessageRead.Where(x => x.LastMessageId >= messageDb.Id && x.UserName != accUser).CountAsync();
                }

                response.Data = count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
                response.error.Message = ex.Message;
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// Thông tin room chat
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("group-info")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessage<GroupInfoResp>), description: "Group Info Message Response")]
        public async Task<IActionResult> GetGroupInfo([FromBody] UuidRequest request)
        {
            var accUser = User.GetUserName();
            var response = new BaseResponseMessage<GroupInfoResp>();

            try
            {
                var roomDb = await _context.Rooms
                    .AsNoTracking()
                    .Include(x => x.RegisterAutoDelete.Where(x => x.UserName == accUser))
                    .Include(x => x.RoomMembers.Where(x => x.InRoom == 1))
                    .Where(x => x.Uuid == request.Uuid)
                    .Where(x => x.RoomMembers.Any(x => x.UserName == accUser && x.InRoom == 1))
                    .FirstOrDefaultAsync();

                if (roomDb == null)
                {
                    response.error.SetErrorCode(ErrorCode.NOT_FOUND);
                    response.error.Message = "Room not found";
                    return new OkObjectResult(response);
                }

                var autoDel = roomDb.RegisterAutoDelete.FirstOrDefault();
                int delValue = 0;
                if (autoDel != null)
                {
                    delValue = autoDel.PeriodTime ?? 0;
                }

                var memberCount = roomDb.RoomMembers.Select(x => x.UserNameNavigation).Count();

                var permissions = roomDb.RoomMembers
                    .Where(x => x.UserName == accUser && x.InRoom == 1)
                    .Select(x => new GroupInfoResp.PermissionDto()
                    {
                        ChangeGroupInfo = x.ChangeGroupInfo,
                        DeleteMessage = x.DeleteMessage,
                        AddMember = x.AddMember,
                        BanUser = x.BanUser,
                        LockMember = x.LockMember
                    }).First();

                var data = new GroupInfoResp
                {
                    MemCount = memberCount,
                    AutoDelete = delValue,
                    RoomMembers = new List<GroupInfoResp.RoomMembersDto>(),
                    Permissions = permissions
                };

                foreach (var roomMember in roomDb.RoomMembers)
                {
                    var accountDb = _context.Account.FirstOrDefault(x => x.UserName == roomMember.UserName);
                    var isFriend = _context.Friends.FirstOrDefault(x => x.UserSent == accUser && x.UserReceiver == roomMember.UserName && x.Status == 3) != null;
                    var canMakeFriend = _context.Friends.FirstOrDefault(x => x.UserSent == accUser && x.UserReceiver == roomMember.UserName && x.Type == 1) != null ? 1 : 0;

                    if (accountDb != null)
                    {
                        data.RoomMembers.Add(new GroupInfoResp.RoomMembersDto
                        {
                            Uuid = accountDb.Uuid,
                            UserName = roomMember.UserName,
                            FullName = accountDb.FullName,
                            Avatar = accountDb.Avatar,
                            TimeCreated = accountDb.TimeCreated,
                            Status = accountDb.Status,
                            RoleId = accountDb.RoleId,
                            RoomRoleId = roomMember.RoleId,
                            IsFriend = isFriend,
                            IsOnline = accountDb.Session.Any(x => x.IsOnline == 1),
                            CanMakeFriend = canMakeFriend,
                        });
                    }
                    else
                    {
                        data.RoomMembers.Add(new GroupInfoResp.RoomMembersDto
                        {
                            UserName = roomMember.UserName,
                        });
                    }
                }

                response.Data = data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);

                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
                response.error.Message = ex.Message;
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// Đăng ký xoá tin nhắn tự động
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("auto-delete-message")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessage<string>), description: "Register Auto Delete Message Response")]
        public async Task<IActionResult> RegisterAutoDeleteMsg([FromBody] AutoDeleteRequest request)
        {
            var accUser = User.GetUserName();
            var accUuid = User.GetAccountUuid();
            var accFullName = User.GetFullName();

            var response = new BaseResponseMessage<string>();

            try
            {
                //var time3Am = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + "T03:00:00");
                var lastTimeDelete = DateTime.Now;

                var roomDb = await _context.Rooms
                    .Include(x => x.RegisterAutoDelete)
                    .Include(x => x.RoomMembers.Where(x => x.InRoom == 1))
                    .Where(x => x.Uuid == request.RoomUuid)
                    .Where(x => x.RoomMembers.Any(x => x.UserName == accUser && x.InRoom == 1))
                    .FirstOrDefaultAsync();

                if (roomDb == null)
                {
                    response.error.SetErrorCode(ErrorCode.NOT_FOUND);
                    response.error.Message = "Room not found";
                    return new OkObjectResult(response);
                }

                if (roomDb.Type == 1)
                {
                    var autoDel = roomDb.RegisterAutoDelete.FirstOrDefault(x => x.UserName == accUser);
                    if (autoDel != null)
                    {
                        autoDel.PeriodTime = request.Period;
                        autoDel.LastTimeDelete = lastTimeDelete;
                        _context.Update(autoDel);
                    }
                    else
                    {
                        await _context.RegisterAutoDelete.AddAsync(new RegisterAutoDelete
                        {
                            RoomUuid = request.RoomUuid,
                            UserName = accUser,
                            PeriodTime = request.Period,
                            LastTimeDelete = lastTimeDelete,
                        });
                    }
                }
                else
                {
                    var lstUser = roomDb.RoomMembers.ToList();
                    foreach (var user in lstUser)
                    {
                        var autoDel = roomDb.RegisterAutoDelete.FirstOrDefault(x => x.UserName == user.UserName);
                        if (autoDel != null)
                        {
                            autoDel.PeriodTime = request.Period;
                            autoDel.LastTimeDelete = lastTimeDelete;

                            _context.Update(autoDel);
                        }
                        else
                        {
                            await _context.RegisterAutoDelete.AddAsync(new RegisterAutoDelete
                            {
                                RoomUuid = request.RoomUuid,
                                UserName = user.UserName,
                                PeriodTime = request.Period,
                                LastTimeDelete = lastTimeDelete
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
                response.error.Message = ex.Message;
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// Get thumbnail
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("get-thumbnail-from-url")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessage<string>), description: "")]
        public async Task<IActionResult> GetThumbnailFromUrl([FromBody] GetThumbnailFromUrlRequest request)
        {
            //var accUser = User.GetUserName();
            //var accUuid = User.GetAccountUuid();
            //var accFullName = User.GetFullName();

            var response = new BaseResponseMessage<object>();

            try
            {
                var webpageInfo = await Helper.GetWebpageInfo(request.Url);

                response.Data = webpageInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
                response.error.Message = ex.Message;
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// Tìm trong Danh sách tin nhắn
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("search-in-message-line")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessagePage<string>), description: "Load Messages In Line Response")]
        public async Task<IActionResult> SearchInMessageLine([FromBody] LoadMessageRequest request)
        {
            var accUser = User.GetUserName();
            var response = new BaseResponseMessagePage<string>();

            try
            {
                var results = await _context.Messages
                    .AsNoTracking()
                    .Where(x => x.RoomUuid == request.MsgGroupUuid)
                    .Where(x => x.Status != 4)
                    //.Where(x => !x.MessageDelete.Any(x => x.UserName == accUser))
                    .Where(x => x.Status != 3
                        || x.RoomUu.RoomMembers.Any(x => x.UserName == accUser && x.RoleId != 3))
                    .Where(x => x.RoomUu.RoomMembers.Any(x => x.UserName == accUser && x.InRoom == 1))
                    .Where(x => !x.RoomUu.RoomDelete.Any(x => x.UserName == accUser)
                        || x.RoomUu.RoomDelete.OrderByDescending(x => x.Id).First(x => x.UserName == accUser).LastMessageId < x.Id)
                    .Where(x => string.IsNullOrEmpty(request.Keyword) || EF.Functions.Like(x.Content, $"%{request.Keyword}%"))
                    .Where(x => string.IsNullOrEmpty(request.Keyword) ||
                        !(
                            x.ContentType == (int)edContentType.IMAGE
                            || x.ContentType == (int)edContentType.AUDIO
                            || x.ContentType == (int)edContentType.VIDEO
                        )
                    //|| EF.Functions.Like(x.FileInfo ?? "", !string.IsNullOrEmpty(request.Keyword) ? $"%\"fileName\":\"%{request.Keyword}%\"%" : $"%{request.Keyword}%") // là file thì check file name
                    )
                    .Where(x => !(x.MessageDelete != null ? x.MessageDelete.Any(x2 => x2.UserName == accUser) : false)) // bỏ tin nhắn xoá đối với tôi
                    .OrderByDescending(x => x.Id)
                    .Select(x => x.Uuid)
                    .TakePage(request.Page, request.PageSize);

                response.Items = results;
                response.Pagination = new BaseResponseMessagePage<string>.Paginations
                {
                    TotalCount = results.TotalCount,
                    TotalPage = results.TotalPages,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
                response.error.Message = ex.Message;
            }

            return new OkObjectResult(response);
        }

        static private string EncodeBase64(string value)
        {
            var valueBytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(valueBytes);
        }

        static private string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
