using ApiBuyerMorgan.Extensions;
using ChatApp.Enum;
using ChatApp.Extensions;
using ChatApp.Models.DataInfo;
using ChatApp.Models.Request;
using ChatApp.Models.Response;
using ChatApp.Socket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Services.Account;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System.Text;
using TWChatAppApiMaster.Databases.ChatApp;
using TWChatAppApiMaster.Models.Request;
using TWChatAppApiMaster.Socket;
using TWChatAppApiMaster.Utils;
using static ChatApp.Enums.EnumDatabase;
using static ChatApp.Socket.ChatHandler;
using static TWChatAppApiMaster.Models.Response.Admin.GroupGetListResp;

namespace ChatApp.Controllers.v1
{
    [Authorize]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class GroupController : ControllerBase
    {
        private readonly DBContext _context;
        private readonly ILogger<GroupController> _logger;

        public GroupController(DBContext context, ILogger<GroupController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Tạo nhóm chat
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("create-group")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessage<CreategRoupResp>), description: "Create Group Response")]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
        {
            var accUser = User.GetUserName();
            var accUuid = User.GetAccountUuid();
            var accRole = User.GetRoleId();
            var response = new BaseResponseMessage<CreategRoupResp>();

            if (accRole == "0")
            {
                response.error.SetErrorCode(ErrorCode.PERMISION_DENIED);
                return new OkObjectResult(response);
            }

            if (request.MemberUuids == null || !request.MemberUuids.Any())
            {
                response.error.SetErrorCode(ErrorCode.INVALID_PARAM);
                return new OkObjectResult(response);
            }

            request.MemberUuids.Add(accUuid);

            try
            {
                var newRoom = new Rooms
                {
                    Uuid = Guid.NewGuid().ToString(),
                    RoomName = request.GroupName,
                    Creater = accUser,
                    Avatar = request.GroupAvatar,
                    Type = 2,
                    Status = 1,
                };
                await _context.Rooms.AddAsync(newRoom);

                var accounts = await _context.Account
                    .AsNoTracking()
                    .Where(x => request.MemberUuids.Contains(x.Uuid))
                    .ToListAsync();

                var newMembers = accounts
                    .Select(x => new RoomMembers
                    {
                        UserName = x.UserName,
                        RoomUuid = newRoom.Uuid,
                        RoleId = (sbyte)(x.RoleId == 0 ? 3 : 1),
                    })
                    .ToList();
                await _context.RoomMembers.AddRangeAsync(newMembers);

                await _context.SaveChangesAsync();

                var lstUser = accounts.Where(x => x.UserName != accUser).Select(x => x.UserName).ToList();

                var data = new JoinGroupMessage
                {
                    GroupLeader = accUser
                };
                var responseData = new TWChatAppApiMaster.Socket.Message
                {
                    MsgType = (int)MessageType.TYPE_JOIN_GROUP,
                    Data = JsonConvert.SerializeObject(data)
                };
                await ChatHandler.getInstance().SendMessageToGroupUsersAsync(JsonConvert.SerializeObject(responseData), lstUser);

                response.Data = new CreategRoupResp
                {
                    RoomUuid = newRoom.Uuid,
                    OwnerUuid = newRoom.Uuid
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

        /// <summary>
        /// Thêm/xoá thành viên/ rời nhóm
        /// </summary>
        /// <param name="request"></param>
        /// <remarks>
        ///     - Type:
        ///         + 0: Xoá thành viên/ rời nhóm
        ///         + 1: Thêm thành viên
        /// </remarks>
        /// <returns></returns>
        [HttpPost("upsert-group-member")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessage<string>), description: "Upsert Group Member Response")]
        public async Task<IActionResult> UpsertGroupMember([FromBody] UpsertGroupMemberRequest request)
        {
            var accUser = User.GetUserName();
            var accUuid = User.GetAccountUuid();
            var accRole = User.GetRoleId();
            var response = new BaseResponseMessage<string>();

            try
            {
                var roomDb = await _context.Rooms
                    .AsNoTracking()
                    .Include(x => x.RoomMembers)
                    .Where(x => x.Type == 2 && x.Status != 4)
                    .Where(x => x.Uuid == request.GroupUuid)
                    .Where(x => x.RoomMembers.Any(x => x.UserName == accUser && x.InRoom == 1))
                    .FirstOrDefaultAsync();

                if (roomDb == null)
                {
                    response.error.SetErrorCode(ErrorCode.NOT_FOUND);
                    response.error.Message = "Nhóm không tồn tại";
                    return new OkObjectResult(response);
                }

                var roleInRoom = roomDb.RoomMembers.First(x => x.UserName == accUser).RoleId;

                if (request.ListNewMemberUuid == null || request.ListNewMemberUuid.Count == 0)
                {
                    response.error.SetErrorCode(ErrorCode.ACCOUNT_NF);
                    return new OkObjectResult(response);
                }

                var userMembers = new List<string>();

                foreach (var newMemberUuid in request.ListNewMemberUuid)
                {
                    var accountDb = await _context.Account.AsNoTracking().FirstOrDefaultAsync(x => x.Uuid == newMemberUuid);

                    if (accountDb == null)
                    {
                        response.error.SetErrorCode(ErrorCode.ACCOUNT_NF);
                        return new OkObjectResult(response);
                    }

                    var member = roomDb.RoomMembers.FirstOrDefault(x => x.UserName == accountDb.UserName);

                    // Thêm member vào nhóm
                    if (request.Type == 1)
                    {
                        var blockDb = roomDb.RoomBlock.FirstOrDefault(x => x.UserName == accountDb.UserName && x.State == (int)edBlockState.BLOCK);//Kiểm tra xem tk có bị block không
                        if (blockDb != null)
                        {
                            response.error.SetErrorCode(ErrorCode.USER_BLOCK);
                            return new OkObjectResult(response);
                        }

                        var isAllow = await GroupService.CheckPermission(request.GroupUuid, accUser, edTypeGroupPermisson.ADD_MEMBER);// Kiểm tra có được gắn chức năng này không
                        if ((!isAllow && roleInRoom != 1) || newMemberUuid == accUuid)
                        {
                            response.error.SetErrorCode(ErrorCode.PERMISION_DENIED);
                            return new OkObjectResult(response);
                        }

                        if (member != null)
                        {
                            if (member.InRoom == 1)
                            {
                                response.error.SetErrorCode(ErrorCode.USER_EXIST_IN_DROUP);
                                return new OkObjectResult(response);
                            }

                            member.InRoom = 1;
                            _context.RoomMembers.Update(member);
                        }
                        else
                        {
                            member = new RoomMembers()
                            {
                                UserName = accountDb.UserName,
                                RoomUuid = roomDb.Uuid,
                                RoleId = (sbyte)(accountDb.RoleId == 0 ? 3 : 1),
                            };
                            await _context.RoomMembers.AddAsync(member);
                        }
                    }
                    else
                    {
                        if (roleInRoom != 1 && newMemberUuid != accUuid)
                        {
                            response.error.SetErrorCode(ErrorCode.PERMISION_DENIED);
                            return new OkObjectResult(response);
                        }

                        // Xoá thành viên hoặc Member rời nhóm
                        if ((roleInRoom == 1 && newMemberUuid != accUuid)
                            || roleInRoom != 1 && newMemberUuid == accUuid)
                        {
                            var isAllow = await GroupService.CheckPermission(request.GroupUuid, accUser, edTypeGroupPermisson.BAN_USER);// Kiểm tra có được gắn chức năng này không
                            if (!isAllow && roleInRoom != 1 && newMemberUuid != accUuid)
                            {
                                response.error.SetErrorCode(ErrorCode.PERMISION_DENIED);
                                return new OkObjectResult(response);
                            }

                            if (member == null || member.InRoom == 0)
                            {
                                response.error.SetErrorCode(ErrorCode.ACCOUNT_NF);
                                return new OkObjectResult(response);
                            }

                            member.InRoom = 0;
                            member.ChangeGroupInfo = 0;
                            member.DeleteMessage = 0;
                            member.LockMember = 0;
                            member.AddMember = 0;
                            member.BanUser = 0;
                            _context.RoomMembers.Update(member);

                            var roomPinDb = await _context.RoomPin.Where(x => x.RoomUuid == roomDb.Uuid && x.UserName == member.UserName).FirstOrDefaultAsync();
                            if (roomPinDb != null && roomPinDb.State == 1)
                            {
                                roomPinDb.State = 0;
                            }

                        }
                        else // Trưởng nhóm rời nhóm
                        {
                            var isAllow = await GroupService.CheckPermission(request.GroupUuid, accUser, edTypeGroupPermisson.BAN_USER);// Kiểm tra có được gắn chức năng này không
                            if (!isAllow && roleInRoom != 1 && newMemberUuid != accUuid)
                            {
                                response.error.SetErrorCode(ErrorCode.PERMISION_DENIED);
                                return new OkObjectResult(response);
                            }

                            if (member == null || member.InRoom == 0)
                            {
                                response.error.SetErrorCode(ErrorCode.ACCOUNT_NF);
                                return new OkObjectResult(response);
                            }

                            member.InRoom = 0;
                            member.ChangeGroupInfo = 0;
                            member.DeleteMessage = 0;
                            member.LockMember = 0;
                            member.AddMember = 0;
                            member.BanUser = 0;
                            _context.RoomMembers.Update(member);

                            var roomPinDb = await _context.RoomPin.Where(x => x.RoomUuid == roomDb.Uuid && x.UserName == member.UserName).FirstOrDefaultAsync();
                            if (roomPinDb != null && roomPinDb.State == 1)
                            {
                                roomPinDb.State = 0;
                            }
                            //roomDb.Status = 4;
                            //var members = roomDb.RoomMembers.Where(x => x.InRoom == 1).ToList();
                            //members.ForEach(x => x.InRoom = 0);

                            //_context.RoomMembers.UpdateRange(members);

                            //var roomPinDb = await _context.RoomPin.Where(x => x.RoomUuid == roomDb.Uuid && x.State == 1).ToListAsync();
                            //if (roomPinDb.Any())
                            //{
                            //    roomPinDb.ForEach(x => x.State = 0);
                            //}

                            //userMembers.AddRange(members.Select(x => x.UserName).ToList());
                        }
                    }
                }

                string data;

                if (request.Type == 1)
                {
                    data = JsonConvert.SerializeObject(new JoinGroupMessage
                    {
                        GroupLeader = accUser
                    });
                }
                else
                {
                    data = JsonConvert.SerializeObject(new LeaveGroupMessage
                    {
                        RoomUuid = request.GroupUuid
                    });
                }

                List<string> listAccountDb = new List<string>();

                if (request.ListNewMemberUuid != null && request.ListNewMemberUuid.Count > 0)
                {
                    listAccountDb = _context.Account
                    .AsNoTracking()
                    .Where(x => (request.ListNewMemberUuid ?? new List<string>()).Contains(x.Uuid))
                    .Select(x => x.UserName)
                    .ToList();
                }

                await _context.SaveChangesAsync();

                // Gửi socket
                var responseData = new TWChatAppApiMaster.Socket.Message
                {
                    MsgType = request.Type == 1 ? (int)MessageType.TYPE_JOIN_GROUP : (int)MessageType.TYPE_LEAVE_GROUP,
                    Data = data,
                };
                if (userMembers.Any())
                    await ChatHandler.getInstance().SendMessageToGroupUsersAsync(JsonConvert.SerializeObject(responseData), userMembers);
                else
                    await ChatHandler.getInstance().SendMessageToGroupUsersAsync(JsonConvert.SerializeObject(responseData), listAccountDb);
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
        /// Chi tiết nhóm
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("detail-group")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessage<GroupDetailDTO>), description: "Group Detail Response")]
        public async Task<IActionResult> GroupDetail([FromBody] UuidRequest request)
        {
            var response = new BaseResponseMessage<GroupDetailDTO>();

            try
            {
                var roomDb = await _context.Rooms
                    .AsNoTracking()
                    .Where(x => x.Uuid == request.Uuid)
                    .Select(x => new GroupDetailDTO
                    {
                        Uuid = x.Uuid,
                        GroupName = x.RoomName ?? "",
                        TimeCreated = x.TimeCreated,
                        Status = x.Status,
                        Avatar = x.Avatar,
                        NumberOfMember = x.RoomMembers.Count(x => x.InRoom == 1),
                    })
                    .FirstOrDefaultAsync();

                if (roomDb == null)
                {
                    response.error.SetErrorCode(ErrorCode.INVALID_PARAM);
                    return new OkObjectResult(response);
                }

                response.Data = roomDb;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// Danh sách thành viên
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("list-member")]
        [SwaggerResponse(statusCode: 200, type: typeof(FindMemberResponseMessage<MemberDetailDTO>), description: "List Members Of Group Response")]
        public async Task<IActionResult> ListMembers([FromBody] UuidAndKeywordAndIsLockRequest request)
        {
            var accUser = User.GetUserName();
            var response = new FindMemberResponseMessage<MemberDetailDTO>();

            try
            {
                var queryableRoomMembers = _context.RoomMembers
                    .AsNoTracking()
                    .Where(x => x.RoomUu.Status != 4)
                    .Where(x => x.RoomUuid == request.Uuid && (request.IsBlock == 1 ? true : x.InRoom == 1))
                    .Where(x => string.IsNullOrEmpty(request.Keyword)
                        || EF.Functions.Like(x.UserName, $"%{request.Keyword}%")
                        || EF.Functions.Like(x.UserNameNavigation.FullName ?? "", $"%{request.Keyword}%"))
                    .OrderBy(x => x.RoleId)
                        .ThenByDescending(x => x.Id);

                var bans = await _context.RoomBan.AsNoTracking().Where(x => x.RoomUuid == request.Uuid && x.State == 1).Select(x => x.UserName).ToListAsync();
                var blocks = await _context.RoomBlock.AsNoTracking().Where(x => x.RoomUuid == request.Uuid && x.State == 1).Select(x => x.UserName).ToListAsync();

                var accMember = await queryableRoomMembers.Where(x => x.UserName == accUser).FirstOrDefaultAsync();
                if (accMember == null)
                {
                    response.error.SetErrorCode(ErrorCode.PERMISION_DENIED);
                    return new OkObjectResult(response);
                }

                var results = await queryableRoomMembers
                    //.Where(x => request.IsBan == null)
                    //.Where(x => request.IsBan == null || bans.Contains(x.UserName) == (request.IsBan == 1))
                    .Include(x => x.UserNameNavigation)
                    .Where(x => request.IsBan == null || bans.Contains(x.UserName) == (request.IsBan == 1))
                    .Where(x => request.IsBlock == null || blocks.Contains(x.UserName) == (request.IsBlock == 1))
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(x => new MemberDetailDTO()
                    {
                        Avatar = x.UserNameNavigation.Avatar,
                        FullName = x.UserNameNavigation.FullName,
                        Uuid = x.UserNameNavigation.Uuid,
                        UserName = x.UserName,
                        TimeCreated = x.UserNameNavigation.TimeCreated,
                        RoleId = x.RoleId,
                        Status = x.UserNameNavigation.Status,
                        IsFriend = _context.Friends.Where(f => f.Status == 3).Any(f => (f.UserSent == accUser && f.UserReceiver == x.UserName) || (f.UserSent == x.UserName && f.UserReceiver == accUser)),
                        IsOnline = x.UserNameNavigation.Session.Any(x => x.TimeExpired > DateTime.UtcNow && x.Status == 0 && x.IsOnline == 1),
                        CanMakeFriend = (int)x.CanMakeFriend,
                        IsBan = bans.Contains(x.UserName),
                        IsBlock = blocks.Contains(x.UserName),
                    })
                    .ToListAsync();

                foreach (var item in results)
                {
                    var friendDb = _context.Friends
                        .FirstOrDefault(f => (f.UserSent == accUser && f.UserReceiver == item.UserName) || (f.UserSent == item.UserName && f.UserReceiver == accUser));

                    if (friendDb == null || friendDb.Type == 2) // Không có hoặc block
                    {
                        item.FriendRequestStatus = 1; // ko ban bè
                        item.FriendRequestSendStatus = null; // không có dữ liệu
                    }
                    else
                    {
                        item.FriendRequestStatus = friendDb.Status; // 1: ko bạn bè;  2: Chờ xác nhận;  3: Bạn bè;
                        if (friendDb.UserSent == item.UserName)
                        {
                            item.FriendRequestSendStatus = (sbyte)edFriendRequestSendStatus.SEND; // 0: nhận, 1: gửi
                        }
                        else
                        {
                            item.FriendRequestSendStatus = (sbyte)edFriendRequestSendStatus.RECEIVE; // 0: nhận, 1: gửi
                        }

                    }
                }

                response.Items = results;
                response.TotalCount = _context.RoomMembers
                    .Where(x => x.RoomUu.Status != 4)
                    .Where(x => x.RoomUuid == request.Uuid && x.InRoom == 1)
                    .Select(x => new MemberDetailDTO()
                    {
                        Avatar = x.UserNameNavigation.Avatar,
                    })
                    .ToList()
                    .Count;
                response.TotalBan = _context.RoomMembers
                    .Where(x => x.RoomUu.Status != 4)
                    .Where(x => x.RoomUuid == request.Uuid && x.InRoom == 1)
                    .Where(x => bans.Contains(x.UserName))
                    .Select(x => new MemberDetailDTO()
                    {
                        Avatar = x.UserNameNavigation.Avatar,
                    })
                    .ToList()
                    .Count;
                response.TotalBlock = _context.RoomMembers
                    .Where(x => x.RoomUu.Status != 4)
                    .Where(x => x.RoomUuid == request.Uuid)
                    .Where(x => blocks.Contains(x.UserName))
                    .Select(x => new MemberDetailDTO()
                    {
                        Avatar = x.UserNameNavigation.Avatar,
                    })
                    .ToList()
                    .Count;
                response.MakeFriendState = (int)accMember.CanMakeFriend;
                response.IsGroupAdmin = accMember.RoleId == 1 ? 1 : 0;
                response.Leader = queryableRoomMembers
                    .Select(x => new MemberDetailDTO()
                    {
                        Avatar = x.UserNameNavigation.Avatar,
                        FullName = x.UserNameNavigation.FullName,
                        Uuid = x.UserNameNavigation.Uuid,
                        UserName = x.UserName,
                        TimeCreated = x.UserNameNavigation.TimeCreated,
                        RoleId = x.RoleId,
                        Status = x.UserNameNavigation.Status,
                        IsFriend = _context.Friends.Where(f => f.Status == 3).Any(f => (f.UserSent == accUser && f.UserReceiver == x.UserName) || (f.UserSent == x.UserName && f.UserReceiver == accUser)),
                        IsOnline = x.UserNameNavigation.Session.Any(x => x.TimeExpired > DateTime.UtcNow && x.Status == 0 && x.IsOnline == 1),
                        CanMakeFriend = (int)x.CanMakeFriend,
                        IsBan = bans.Contains(x.UserName),
                        IsBlock = blocks.Contains(x.UserName),
                    })
                    .FirstOrDefault(x => x.RoleId == (int)edRoomMember.OWNER);
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
        /// Danh sách tin nhắn media
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("list-media")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessagePage<MessageLineDTO>), description: "List Media files Of Group Response")]
        public async Task<IActionResult> ListMediaInGroup([FromBody] GetListFileInGroupRequest request)
        {
            var accUser = User.GetUserName();
            var response = new BaseResponseMessagePage<MessageLineDTO>();

            try
            {
                var _results = MessengeExtension.GetMessagesAvailableByRoom(_context.Messages, accUser, request.Uuid);
                var results = await _results
                    .Where(x => x.ContentType == request.Type) //lấy tin nhắn với kiểu chỉ định
                    .OrderByDescending(x => x.Id)
                    .Select(item => new MessageLineDTO()
                    {
                        Uuid = item.Uuid,
                        UserSent = item.UserSent,
                        UserSentUuid = item.UserSentNavigation.Uuid,
                        Content = item.Content,
                        ContentType = item.ContentType,
                        LastEdited = item.LastEdited,
                        TimeCreated = item.TimeCreated,
                        Status = item.Status,
                        FileInfo = item.FileInfo != null ? EncodeBase64(item.FileInfo) : null,
                        MsgRoomUuid = item.RoomUuid,
                    })
                    .TakePage(request.Page, request.PageSize);

                foreach (var item in results.Where(x => x.ContentType == 4))
                {
                    var file = item.Content.Substring(2, item.Content.Length - 4);
                    var fileName = await _context.FilesInfo.Where(x => x.Path == file).Select(x => x.FileName).FirstOrDefaultAsync();
                    item.MediaName = fileName ?? "";
                }

                response = new()
                {
                    Items = results,
                    Pagination = new()
                    {
                        TotalCount = results.TotalCount,
                        TotalPage = results.TotalPages
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// Thay đổi trạng thái cho phép kết bạn
        /// </summary>
        /// <param name="request"></param>
        /// <remarks>
        ///     - Type:
        ///         + 0: Không cho phép
        ///         + 1: Cho phép
        /// </remarks>
        /// <returns></returns>
        [HttpPost("change-make-friend-state")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponse), description: "Change make friend state of member in Response")]
        public async Task<IActionResult> ChangeMakeFriendState([FromBody] ChangeMakeFriendStateRequest request)
        {
            var accUser = User.GetUserName();
            var response = new BaseResponse();

            try
            {
                var members = await _context.RoomMembers
                    .Include(x => x.UserNameNavigation)
                    .Where(x => x.RoomUuid == request.Uuid)
                    .Where(x => x.UserName == accUser || x.UserNameNavigation.Uuid == request.MemberUuid)
                    .Where(x => x.RoomUu.Status != 4 && x.InRoom == 1)
                    .ToListAsync();

                if (!members.Any(x => x.UserName == accUser && x.RoleId == 1))
                {
                    response.error.SetErrorCode(ErrorCode.PERMISION_DENIED);
                    return new OkObjectResult(response);
                }

                var memberChange = members.FirstOrDefault(x => x.UserNameNavigation.Uuid == request.MemberUuid);
                if (memberChange == null)
                {
                    response.error.SetErrorCode(ErrorCode.ACCOUNT_NF);
                    return new OkObjectResult(response);
                }

                memberChange.CanMakeFriend = (ulong)request.State;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// Thay đổi thông tin nhóm
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("change-group-info")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessage<string>), description: "Change make friend state of member in Response")]
        public async Task<IActionResult> ChangeGroupInfo([FromBody] ChangeGroupInfoRequest request)
        {
            var accUser = User.GetUserName();
            var accUuid = User.GetAccountUuid();

            var response = new BaseResponseMessage<string>();

            try
            {
                var roomDb = await _context.Rooms
                    .Include(x => x.RoomMembers.Where(x => x.UserName == accUser))
                    .Where(x => x.Status != 4)
                    .Where(x => x.Uuid == request.Uuid)
                    .FirstOrDefaultAsync();

                if (roomDb == null)
                {
                    response.error.SetErrorCode(ErrorCode.NOT_FOUND);
                    return new OkObjectResult(response);
                }

                var isAllow = await GroupService.CheckPermission(request.Uuid, accUser, edTypeGroupPermisson.CHANGE_GROUP_INFO);// Kiểm tra có được gắn chức năng này không
                if (!isAllow && !roomDb.RoomMembers.Any(x => x.RoleId == 1))
                {
                    response.error.SetErrorCode(ErrorCode.PERMISION_DENIED);
                    return new OkObjectResult(response);
                }

                if (!string.IsNullOrEmpty(request.GroupName))
                    roomDb.RoomName = request.GroupName;
                if (!string.IsNullOrEmpty(request.GroupAvatar))
                    roomDb.Avatar = request.GroupAvatar;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            response.Data = response.error.Code.ToDescriptionString();
            return new OkObjectResult(response);
        }

        /// <summary>
        /// Gắn chức năng cho thành viên
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("assign-permission-for-member")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponse), description: "Assign permission for member")]
        public async Task<IActionResult> AssignPermissionForMember([FromBody] AssignPermissionForMemberRequest request)
        {
            var response = new BaseResponse();
            var accUser = User.GetUserName();
            var accUuid = User.GetAccountUuid();

            try
            {
                var roomDb = await _context.Rooms.AsNoTracking()
                    .Include(x => x.RoomMembers)
                    .Where(x => x.Status != 4)
                    .Where(x => x.Uuid == request.Uuid)
                    .FirstOrDefaultAsync();
                if (roomDb == null)
                {
                    response.error.SetErrorCode(ErrorCode.NOT_FOUND);
                    return new OkObjectResult(response);
                }
                if (!roomDb.RoomMembers.Any(x => x.RoleId == 1))
                {
                    response.error.SetErrorCode(ErrorCode.PERMISION_DENIED);
                    return new OkObjectResult(response);
                }
                if (request.UserUuid == accUuid)
                {
                    response.error.SetErrorCode(ErrorCode.PERMISION_DENIED);
                    return new OkObjectResult(response);
                }

                var mem = await _context.RoomMembers.FirstOrDefaultAsync(x => x.RoomUuid == request.Uuid && x.UserNameNavigation.Uuid == request.UserUuid && x.InRoom == 1);
                if (mem is null)
                {
                    response.error.SetErrorCode(ErrorCode.NOT_FOUND);
                    return new OkObjectResult(response);
                }

                if (request.ChangeGroupInfo.HasValue) { mem.ChangeGroupInfo = request.ChangeGroupInfo.Value ? 1u : 0u; }
                if (request.ChangeGroupInfo.HasValue) { mem.DeleteMessage = request.DeleteMessage.Value ? 1u : 0u; }
                if (request.ChangeGroupInfo.HasValue) { mem.BanUser = request.BanUser.Value ? 1u : 0u; }
                if (request.ChangeGroupInfo.HasValue) { mem.AddMember = request.AddMember.Value ? 1u : 0u; }
                if (request.ChangeGroupInfo.HasValue) { mem.LockMember = request.LockMember.Value ? 1u : 0u; }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// Danh sách nhóm chat recent(vào gần đây)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("recent-rooms")]
        [SwaggerResponse(statusCode: 200, type: typeof(MessageLineResponseMessage<MessagesGroupDTO>), description: "List recently rooms")]
        public async Task<IActionResult> GetRecentRooms([FromBody] RecentRoomsRequest request)
        {
            var accUser = User.GetUserName();
            var response = new MessageLineResponseMessage<MessagesGroupDTO>();

            try
            {
                var results = await _context.RoomRecent
                    .AsNoTracking()
                    .Include(x => x.RoomUu)
                    .Where(x => x.Count > 0)
                    .Where(x => x.UserName == accUser) // là người gọi
                    .Where(x => x.RoomUu.Status != 4) // nhóm chưa bị xoá
                    .Where(x => x.RoomUu.RoomMembers.Any(x2 => x2.UserName == accUser && x2.InRoom == 1)) // người gọi ở trong nhóm
                    .OrderByDescending(x => x.Count) // lấy nhóm có số đếm cao
                    .TakePage(request.Page, request.PageSize);

                response.Items.AddRange(results.Select(x =>
                {
                    var item = new MessagesGroupDTO
                    {
                        Uuid = x.RoomUu.Uuid,
                        Type = x.RoomUu.Type,
                        LastUpdated = x.RoomUu.LastUpdated,
                        TimeCreated = x.RoomUu.TimeCreated,
                        Pinned = x.RoomUu.RoomPin.Any(rp => rp.UserName == accUser && rp.State == 1),
                    };

                    if (x.RoomUu.Type == 2)
                    {
                        item.UserSent = "admin";
                        item.Content = x.RoomUu.CreaterNavigation.FullName ?? x.RoomUu.CreaterNavigation.UserName;
                        item.ContentType = 1;
                        item.UnreadCount = 0;
                        item.LastUpdated = x.RoomUu.TimeCreated;
                    }

                    if (x.RoomUu.Type == 1)
                    {
                        var member = x.RoomUu.RoomMembers.First(rm => rm.UserName != accUser).UserNameNavigation;
                        item.Avatar = member.Avatar;
                        item.OwnerUuid = member.Uuid;
                        item.PartnerUuid = member.Uuid;
                        item.OwnerName = x.RoomUu.CreaterNavigation.FullName ?? x.RoomUu.CreaterNavigation.UserName;
                    }
                    else
                    {
                        item.Avatar = x.RoomUu.Avatar;
                        item.OwnerUuid = x.RoomUu.Uuid;
                        item.OwnerName = x.RoomUu.RoomName;
                    }

                    return item;
                }).ToList());
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
        /// Thêm hoạt động cho danh sách gần đây 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("trigger-recent")]
        [SwaggerResponse(statusCode: 200, type: typeof(MessageLineResponseMessage<string>), description: "Trigger add recent")]
        public async Task<IActionResult> TriggerRecent([FromBody] TriggerRecentRoomsRequest request)
        {
            var accUser = User.GetUserName();
            var response = new MessageLineResponseMessage<string>();

            try
            {
                var accountDb = _context.Account.Where(x => x.UserName == accUser);
                var roomDb = _context.Rooms
                    .Include(x => x.RoomRecent)
                    .Where(x => x.Status != 4) // nhóm chưa bị xoá;
                    .Where(x => x.RoomMembers.Any(x2 => x2.UserName == accUser && x2.InRoom == 1)) // người gọi ở trong nhóm; 
                    .FirstOrDefault(x => x.Uuid == request.RoomUuid);

                if (accountDb == null)
                {
                    response.error.SetErrorCode(ErrorCode.ACCOUNT_NF);
                    return new OkObjectResult(response);
                }
                if (roomDb == null)
                {
                    response.error.SetErrorCode(ErrorCode.ROOM_DENY);
                    return new OkObjectResult(response);
                }

                var roomRecentDb = roomDb.RoomRecent
                    .FirstOrDefault(x => x.UserName == accUser && x.RoomUuid == request.RoomUuid);

                if (roomRecentDb == null)
                {
                    var newRoomRecent = new RoomRecent
                    {
                        RoomUuid = request.RoomUuid,
                        UserName = accUser,
                        Count = 1,
                    };
                    _context.RoomRecent.Add(newRoomRecent);
                }
                else
                {
                    roomRecentDb.Count += 1;
                    _context.RoomRecent.Update(roomRecentDb);
                }
                _context.SaveChanges();
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
        /// Làm trắng danh sách gần đây 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("clear-recent")]
        [SwaggerResponse(statusCode: 200, type: typeof(MessageLineResponseMessage<string>), description: "Clear list recent")]
        public async Task<IActionResult> ClearRecent([FromBody] ClearRecentRoomsRequest request)
        {
            var accUser = User.GetUserName();
            var response = new MessageLineResponseMessage<string>();

            try
            {
                var roomRecentDb = _context.RoomRecent
                    .Where(x => x.UserName == accUser)
                    .ToList();

                if (roomRecentDb != null)
                {
                    foreach (var item in roomRecentDb)
                    {
                        item.Count = 0;
                    }

                    _context.RoomRecent.UpdateRange(roomRecentDb);
                }

                _context.SaveChanges();
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
        /// Danh sách chat gần đây
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("get-chats-by-user")]
        [SwaggerResponse(statusCode: 200, type: typeof(MessageLineResponseMessage<MessagesGroupDTO>), description: "List chat by user")]
        public async Task<IActionResult> GetChatsByUser([FromBody] RecentRoomsRequest request)
        {
            var accUser = User.GetUserName();
            var response = new MessageLineResponseMessage<MessagesGroupDTO>();

            try
            {
                //var results = await _context.MessageRead
                //    .AsNoTracking()
                //    .Include(x => x.RoomUu)
                //    .Include(x => x.RoomUu.RoomMembers)
                //    .Include(x => x.RoomUu.CreaterNavigation)
                //    .Where(x => x.RoomUu.Status != 4) // nhóm chưa bị xoá
                //    .Where(x => x.RoomUu.RoomMembers.Any(x2 => x2.UserName == accUser && x2.InRoom == 1)) // người gọi ở trong nhóm
                //    .OrderByDescending(x => x.TimeRead) // xắp xếp theo thời gian đọc
                //    .TakePage(request.Page, request.PageSize);

                var results = _context.MessageRead
                    .AsNoTracking()
                    .Include(x => x.RoomUu)
                    .Include(x => x.RoomUu.RoomMembers)
                    .Include(x => x.RoomUu.CreaterNavigation)
                    .Where(x => x.RoomUu.Status != 4) // nhóm chưa bị xoá
                    .Where(x => x.RoomUu.RoomMembers.Any(x2 => x2.UserName == accUser && x2.InRoom == 1)) // người gọi ở trong nhóm
                    //.OrderByDescending(x => x.TimeRead) // xắp xếp theo thời gian đọc
                    .GroupBy(x => x.RoomUuid)
                    .Select(x => x.OrderByDescending(x => x.TimeRead).FirstOrDefault())
                    .ToList()
                    .OrderByDescending(x => x.TimeRead)
                    .TakePage(request.Page, request.PageSize);

                if (results == null)
                {
                    return new OkObjectResult(response);
                }

                response.Items.AddRange(results.Select(x =>
                {
                    var item = new MessagesGroupDTO
                    {
                        Uuid = x.RoomUu.Uuid,
                        Type = x.RoomUu.Type,
                        LastUpdated = x.RoomUu.LastUpdated,
                        TimeCreated = x.RoomUu.TimeCreated,
                        Pinned = x.RoomUu.RoomPin.Any(rp => rp.UserName == accUser && rp.State == 1),
                    };

                    if (x.RoomUu.Type == 2)
                    {
                        item.UserSent = "admin";
                        item.Content = x.RoomUu.CreaterNavigation.FullName ?? x.RoomUu.CreaterNavigation.UserName;
                        item.ContentType = 1;
                        item.UnreadCount = 0;
                        item.LastUpdated = x.RoomUu.TimeCreated;
                    }

                    if (x.RoomUu.Type == 1)
                    {
                        var member = x.RoomUu.RoomMembers.First(rm => rm.UserName != accUser).UserNameNavigation;
                        if (member != null)
                        {
                            item.Avatar = member.Avatar;
                            item.OwnerUuid = member.Uuid;
                            item.PartnerUuid = member.Uuid;
                            item.OwnerName = x.RoomUu.CreaterNavigation.FullName ?? x.RoomUu.CreaterNavigation.UserName;
                            item.ShowName = member?.FullName ?? member?.UserName;
                            item.ShowUuid = member?.Uuid;
                        }
                    }
                    else
                    {
                        item.Avatar = x.RoomUu.Avatar;
                        item.OwnerUuid = x.RoomUu.Uuid;
                        item.OwnerName = x.RoomUu.RoomName;
                        item.ShowName = x.RoomUu.RoomName;
                        item.ShowUuid = x.RoomUu.Uuid;
                    }

                    return item;
                }).ToList());
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
