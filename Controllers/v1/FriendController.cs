using ApiBuyerMorgan.Extensions;
using ChatApp.Enum;
using ChatApp.Extensions;
using ChatApp.Models.DataInfo;
using ChatApp.Models.Request;
using ChatApp.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using TWChatAppApiMaster.Databases.ChatApp;
using TWChatAppApiMaster.Models.Request;

namespace ChatApp.Controllers.v1
{
    [Authorize]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class FriendController : ControllerBase
    {
        private readonly DBContext _context;
        private readonly ILogger<FriendController> _logger;
        public FriendController(DBContext context, ILogger<FriendController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Gửi yêu cầu kết bạn
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("request-add-friend")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponse), description: "Add Friend Response")]
        public async Task<IActionResult> AddFriend([FromBody] UuidRequest request)
        {
            var accUser = User.GetUserName();

            var response = new BaseResponse();

            try
            {
                var accountDb = await _context.Account
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Uuid == request.Uuid);

                if (accountDb == null)
                {
                    response.error.SetErrorCode(ErrorCode.ACCOUNT_NF);
                    return new OkObjectResult(response);
                }

                if (accountDb.Status != 1)
                {
                    response.error.SetErrorCode(ErrorCode.USER_LOCKED);
                    return new OkObjectResult(response);
                }

                var friend = await _context.Friends
                    .Where(x => (x.UserSent == accUser && x.UserReceiver == accountDb.UserName)
                        || (x.UserSent == accountDb.UserName && x.UserReceiver == accUser))
                    .FirstOrDefaultAsync();

                if (friend != null)
                {
                    if (friend.Type == 2)
                    {
                        response.error.SetErrorCode(ErrorCode.PERMISION_DENIED);
                        return new OkObjectResult(response);
                    }

                    if (friend.Status != 1)
                    {
                        response.error.SetErrorCode(ErrorCode.FRIEND_REQUEST_EXISTS);
                        return new OkObjectResult(response);
                    }

                    friend.UserSent = accUser;
                    friend.UserReceiver = accountDb.UserName;
                    friend.Status = 2;
                    friend.TimeCreated = DateTime.Now;
                }
                else
                {
                    await _context.Friends.AddAsync(new Friends
                    {
                        Uuid = Guid.NewGuid().ToString(),
                        UserSent = accUser,
                        UserReceiver = accountDb.UserName,
                        Type = 1,
                        Status = 2
                    });
                }
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);

                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// Đồng ý kết bạn
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("accept-friend")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponse), description: "Accept Friend Response")]
        public async Task<IActionResult> AcceptFriend([FromBody] UuidRequest request)
        {
            var accUser = User.GetUserName();
            var response = new BaseResponse();

            try
            {
                var friend = await _context.Friends.Where(x => x.UserSentNavigation.Uuid == request.Uuid && x.UserReceiver == accUser && x.Status == 2).FirstOrDefaultAsync();
                if (friend == null)
                {
                    response.error.SetErrorCode(ErrorCode.NOT_FOUND);
                    return new OkObjectResult(response);
                }

                friend.Status = 3;
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// Từ chối kết bạn
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("refuse-friend")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponse), description: "Refuse Ffriend Response")]
        public async Task<IActionResult> RefuseFriend([FromBody] RefuseFriendRequest request)
        {
            var accUser = User.GetUserName();
            var response = new BaseResponse();

            try
            {
                var friend = await _context.Friends
                    .Where(x =>
                        (
                            (x.UserSentNavigation.Uuid == request.Uuid && x.UserReceiver == accUser)
                            || (x.UserReceiverNavigation.Uuid == request.Uuid && x.UserSent == accUser)
                        )
                        && x.Status == 2)
                    .FirstOrDefaultAsync();
                if (friend == null)
                {
                    response.error.SetErrorCode(ErrorCode.NOT_FOUND);
                    return new OkObjectResult(response);
                }

                friend.Status = 1;
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// Danh sách tìm kiếm bạn bè
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("find-friend")]
        [SwaggerResponse(statusCode: 200, type: typeof(FindFriendResponseMessage<FriendDTO>), description: "Find Friends Response")]
        public async Task<IActionResult> FindFriends([FromBody] FindFriendRequest request)
        {
            var accUser = User.GetUserName();
            var response = new FindFriendResponseMessage<FriendDTO>();

            try
            {
                var results = _context.Friends.AsNoTracking()
                    .Where(x => request.Status == null || x.Status == request.Status)
                    .Where(x => x.UserSent == accUser || (x.Status == 3 && x.UserReceiver == accUser))
                    .Where(x => string.IsNullOrEmpty(request.Keyword)
                        || (accUser == x.UserSent && EF.Functions.Like(x.UserReceiverNavigation.FullName, $"%{request.Keyword}%"))
                        || (accUser == x.UserReceiver && EF.Functions.Like(x.UserSentNavigation.FullName, $"%{request.Keyword}%"))
                    )
                    .Select(x => new
                    {
                        Friend = x,
                        Account = x.UserSent == accUser ? x.UserReceiverNavigation : x.UserSentNavigation,
                    })
                    .AsEnumerable()
                    .OrderByDescending(x => x.Account.Id)
                    .Select(x => new FriendDTO()
                    {
                        Uuid = x.Account.Uuid,
                        FriendUserName = x.Account.UserName,
                        FriendFullName = x.Account.FullName ?? "",
                        Type = x.Friend.Type,
                        LastUpdated = x.Friend.LastUpdated,
                        TimeCreated = x.Friend.TimeCreated,
                        Avatar = x.Account.Avatar ?? "",
                        LastSeen = x.Account.LastSeen,
                        Status = (sbyte)(x.Friend.Status == 1
                            ? 0
                            : x.Friend.Status == 3
                                ? 3
                                : x.Friend.Status == 2 && x.Friend.UserSent == accUser
                                    ? 1 : 2),
                    })
                    .TakePage(request.Page, request.PageSize);

                response = new()
                {
                    Items = results,
                    Count = results.TotalCount,
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
        /// Danh sách bạn bè
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("friend-requests")]
        [SwaggerResponse(statusCode: 200, type: typeof(FindFriendResponseMessage<FriendDTO>), description: "Friends list")]
        public async Task<IActionResult> GetFriendRequests([FromBody] FriendRequestReq request)
        {
            var accUser = User.GetUserName();
            var response = new FindFriendResponseMessage<FriendDTO>();

            try
            {
                var results = _context.Friends.AsNoTracking()
                    .Where(x => x.Status == 2) // Chờ xác nhận
                    .Where(x => request.isSend ? x.UserSent == accUser : x.UserReceiver == accUser)
                    //.Where(x => string.IsNullOrEmpty(request.Keyword)
                    //    || EF.Functions.Like((x.UserSent == accUser ? x.UserReceiverNavigation : x.UserSentNavigation).FullName, $"%{request.Keyword}%"))
                    .Where(x => string.IsNullOrEmpty(request.Keyword)
                        || EF.Functions.Like(x.UserReceiverNavigation.FullName, $"%{request.Keyword}%")
                        || EF.Functions.Like(x.UserSentNavigation.FullName, $"%{request.Keyword}%"))
                    .Select(x => new
                    {
                        Friend = x,
                        Account = x.UserSent == accUser ? x.UserReceiverNavigation : x.UserSentNavigation
                    })
                    .AsEnumerable()
                    .OrderByDescending(x => x.Friend.LastUpdated)
                    .Select(x => new FriendDTO()
                    {
                        Uuid = x.Account.Uuid,
                        FriendUserName = x.Account.UserName,
                        FriendFullName = x.Account.FullName ?? "",
                        Type = x.Friend.Type,
                        LastUpdated = x.Friend.LastUpdated,
                        TimeCreated = x.Friend.TimeCreated,
                        Avatar = x.Account.Avatar ?? "",
                        LastSeen = x.Account.LastSeen,
                        Status = (sbyte)(x.Friend.Status == 1
                            ? 0
                            : x.Friend.Status == 3
                                ? 3
                                : x.Friend.Status == 2 && x.Friend.UserSent == accUser
                                    ? 1 : 2),
                    })
                    .TakePage(request.Page, request.PageSize);

                response = new()
                {
                    Items = results,
                    Count = results.TotalCount,
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
        /// Huỷ kết bạn
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("unfriend")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponse), description: "Unfriend Response")]
        public async Task<IActionResult> Unfriend([FromBody] RefuseFriendRequest request)
        {
            var accUser = User.GetUserName();
            var response = new BaseResponse();

            try
            {
                var friend = await _context.Friends
                    .Where(x =>
                        (
                            (x.UserSentNavigation.Uuid == request.Uuid && x.UserReceiver == accUser)
                            || (x.UserReceiverNavigation.Uuid == request.Uuid && x.UserSent == accUser)
                        )
                        && x.Status == 3)
                    .FirstOrDefaultAsync();
                if (friend == null)
                {
                    response.error.SetErrorCode(ErrorCode.NOT_FOUND);
                    return new OkObjectResult(response);
                }

                friend.Status = 1;
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return new OkObjectResult(response);
        }
    }
}
