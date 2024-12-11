using ChatApp.Enum;
using ChatApp.Models.Request;
using ChatApp.Models.Response;
using ChatApp.Socket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using TWChatAppApiMaster.Databases.ChatApp;
using TWChatAppApiMaster.Models.DataInfo;
using TWChatAppApiMaster.Models.Request.Admin;
using TWChatAppApiMaster.Models.Response.Admin;
using TWChatAppApiMaster.SecurityManagers;

namespace ChatApp.Controllers.v2
{
    [Authorize]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("2.0")]
    [SwaggerTag("Groups Controller")]
    public class ChatRoomsController : ControllerBase
    {
        private readonly DBContext _context;
        private readonly ILogger<ChatRoomsController> _logger;
        private readonly IJwtAuthenticationManager _jwtAuthenticationManager;
        private ConnectionManager _connectionManager { get; set; }
        public ChatRoomsController(DBContext context, ILogger<ChatRoomsController> logger, ConnectionManager ConnectionManager, IJwtAuthenticationManager jwtAuthenticationManager)
        {
            _context = context;
            _logger = logger;
            _connectionManager = ConnectionManager;
            _jwtAuthenticationManager = jwtAuthenticationManager;
        }

        /// <summary>
        /// Danh sách nhóm chat
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("list")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessagePage<GroupGetListResp>), description: "List Members Of Group Response")]
        public async Task<IActionResult> GetListPaging([FromBody] GroupGetRequest request)
        {
            var response = new BaseResponseMessagePage<GroupGetListResp>();

            try
            {
                var queryable = _context.Rooms
                    .AsNoTracking()
                    .Where(x => x.Type == 2)
                    .Where(x => string.IsNullOrEmpty(request.Keyword)
                        || EF.Functions.Like(x.RoomName ?? "", $"%{request.Keyword.Trim()}%"))
                    .Where(x => string.IsNullOrEmpty(request.LeaderUserName)
                        || x.Creater == request.LeaderUserName)
                    .OrderByDescending(x => x.Id);

                var data = await queryable
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(x => new GroupGetListResp()
                    {
                        Id = x.Id,
                        Uuid = x.Uuid,
                        Name = x.RoomName ?? "",
                        MemberCount = x.RoomMembers.Count(x => x.InRoom == 1),
                        Members = x.RoomMembers.Where(x => x.InRoom == 1)
                            .Select(x => new GroupGetListResp.Member
                            {
                                UserName = x.UserName,
                                Avatar = x.UserNameNavigation.Avatar,
                                FullName = x.UserNameNavigation.FullName ?? x.UserName,
                                IsOnline = x.UserNameNavigation.Session.Any(s => s.IsOnline == 1)
                            })
                            .ToList(),

                        Leader = new()
                        {
                            UserName = x.Creater,
                            FullName = x.CreaterNavigation.FullName ?? x.Creater,
                        },
                        TimeCreated = x.TimeCreated,
                        LastUpdated = x.LastUpdated,
                        MessageCount = x.Messages.Count(x => x.Status != 4),
                    })
                    .ToListAsync();

                var totalCount = await queryable.CountAsync();

                var totalPage = totalCount % request.PageSize == 0 
                    ? totalCount / request.PageSize
                    : totalCount / request.PageSize + 1;

                response.Items = data;
                response.Pagination = new()
                {
                    TotalCount = totalCount,
                    TotalPage = totalPage,
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
        /// Danh sách leader
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("leaders")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessagePage<UserNameBaseDTO>), description: "List Members Of Group Response")]
        public async Task<IActionResult> GetLeadersPaging([FromBody] KeywordRequest request)
        {
            var response = new BaseResponseMessagePage<UserNameBaseDTO>();

            try
            {
                var queryable = _context.Account
                    .AsNoTracking()
                    .Where(x => x.RoleId == 1)
                    .Where(x => string.IsNullOrEmpty(request.Keyword)
                        || EF.Functions.Like(x.FullName ?? "", $"%{request.Keyword.Trim()}%"))
                    .OrderByDescending(x => x.Id);

                var data = await queryable
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(x => new UserNameBaseDTO()
                    {
                        UserName = x.UserName,
                        FullName = x.FullName ?? x.UserName,
                    })
                    .ToListAsync();

                var totalCount = await queryable.CountAsync();

                var totalPage = totalCount % request.PageSize == 0
                    ? totalCount / request.PageSize
                    : totalCount / request.PageSize + 1;

                response.Items = data;
                response.Pagination = new()
                {
                    TotalCount = totalCount,
                    TotalPage = totalPage,
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
        /// Đăng nhập với tư cách leader
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("login-with-leader")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessage<LogInResp>), description: "LogIn Response")]
        public async Task<IActionResult> LogInWithLeader([FromBody] LoginWithLeader request)
        {
            var response = new BaseResponseMessage<LogInResp>();
            response.Data = new LogInResp();

            request.UserName = request.UserName.Trim().ToLower();

            try
            {
                var acc = _context.Account.Where(x => x.UserName == request.UserName && x.RoleId == 1).SingleOrDefault();

                if (acc != null)
                {
                    if (acc.ActiveState == 1)
                    {
                        var _authClaims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, acc.UserName),
                            new Claim("AccountUuid", acc.Uuid),
                            new Claim(ClaimTypes.Role, acc.RoleId.ToString() ?? "0"),
                            new Claim("FullName", acc.FullName ?? ""),
                            new Claim("Avatar", acc.Avatar ?? ""),
                        };

                        var _token = _jwtAuthenticationManager.Authenticate(_authClaims, acc.UserName, acc.Uuid, acc.RoleId);

                        if (_token is null)
                        {
                            response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
                            return new OkObjectResult(response);
                        }

                        response.Data.UserName = acc.UserName;
                        response.Data.FullName = acc.FullName;
                        response.Data.Email = acc.Email;
                        response.Data.Token = _token.AccessToken;
                        response.Data.Uuid = acc.Uuid;
                        response.Data.RoleId = acc.RoleId;
                        response.Data.Avatar = acc.Avatar;
                        response.Data.Token = _token.AccessToken;
                        response.Data.RefreshToken = _token.RefreshToken;

                        var newSession = new Session()
                        {
                            Uuid = Guid.NewGuid().ToString(),
                            DeviceId = request.DeviceId,
                            UserName = acc.UserName,
                            LoginTime = DateTime.Now,
                            Ip = request.Ip,
                            FcmToken = request.FcmToken,
                            Status = 0,
                        };

                        _context.Session.Add(newSession);
                        _context.SaveChanges();

                        var newDevice = _context.Devices.Where(x => x.UserName == acc.UserName && x.DeviceId == request.DeviceId).FirstOrDefault();

                        if (newDevice is null)
                        {
                            newDevice = new Devices()
                            {
                                Os = request.Os,
                                DeviceId = request.DeviceId,
                                DeviceName = request.DeviceName,
                                UserName = acc.UserName,
                                LastUsed = DateTime.Now,
                                Ip = request.Ip,
                                Address = request.Address,
                            };

                            _context.Devices.Add(newDevice);
                        }
                        else
                        {
                            newDevice.Ip = request.Ip;
                            newDevice.Address = request.Address;
                            newDevice.LastUsed = DateTime.Now;
                            newDevice.Status = 1;
                        }

                        _context.SaveChanges();
                        _logger.LogInformation($"User {acc.UserName} login from IP {request.Ip}");
                    }
                    else
                    {
                        response.error.SetErrorCode(ErrorCode.USER_LOCKED);
                    }
                }
                else
                {
                    response.error.SetErrorCode(ErrorCode.ACCOUNT_INVALID);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);

                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return Ok(response);
        }
    }
}
