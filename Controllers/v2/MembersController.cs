using ApiBuyerMorgan.Extensions;
using ChatApp.Configuaration;
using ChatApp.Enum;
using ChatApp.Models.Response;
using ChatApp.Socket;
using ChatApp.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.Annotations;
using TWChatAppApiMaster.Databases.ChatApp;
using TWChatAppApiMaster.Models.Request.Admin;
using TWChatAppApiMaster.Models.Response.Admin;
using static ChatApp.Socket.ChatHandler;

namespace TWChatAppApiMaster.Controllers.v2
{
    /// <summary>
    /// Chức năng quản lý thành viên
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")]
    public class MembersController : ControllerBase
    {
        private readonly DBContext _dbContext;
        private readonly ILogger<MembersController> _logger;
        private readonly ConnectionManager _connectionManager;

        public MembersController(DBContext dbContext, ILogger<MembersController> logger, ConnectionManager connectionManager)
        {
            _dbContext = dbContext;
            _logger = logger;
            _connectionManager = connectionManager;
        }

        /// <summary>
        /// Danh sách thành viên
        /// </summary>
        /// <param name="request"></param>
        /// <remarks>
        ///     - Tất cả: isOnline = null
        ///     - Online: isOnline = true
        ///     - Offline: isOnline = false
        ///     #
        ///     - RoleId:
        ///         + 0: User thường
        ///         + 1: Leader
        ///     #
        ///     - ActiveState:
        ///         + 0: Lock
        ///         + 1: Active
        /// </remarks>
        /// <returns></returns>
        [HttpPost("list")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessagePage<MemberGetListResp>), description: "Members Response")]
        public async Task<IActionResult> GetList([FromBody] MemberGetListRequest request)
        {
            var response = new BaseResponseMessagePage<MemberGetListResp>();

            try
            {
                var iQueryable = _dbContext.Account
                    .AsNoTracking()
                    .Where(x => x.RoleId == 0 || x.RoleId == 1)
                    .Where(x => !request.IsOnline.HasValue
                        || x.Session.Any(s => s.IsOnline == 1) == request.IsOnline)
                    .Where(x => !request.RoleId.HasValue
                        || x.RoleId == request.RoleId)
                    .Where(x => string.IsNullOrEmpty(request.Keyword)
                        || EF.Functions.Like(x.FullName ?? x.UserName, $"%{request.Keyword.Trim()}%"))
                    .OrderByDescending(x => x.Session.Any(s => s.IsOnline == 1))
                        .ThenByDescending(x => x.Id);

                var results = await iQueryable
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(x => new MemberGetListResp()
                    {
                        Id = x.Id,
                        Uuid = x.Uuid,
                        FullName = x.FullName ?? x.UserName,
                        TotalMessage = x.Messages.Count(x => x.Status != 4),
                        TotalGroup = x.RoomMembers.Count(x => x.RoomUu.Type == 2 && x.InRoom == 1),
                        IsOnline = x.Session.Any(s => s.IsOnline == 1),
                        TimeCreated = x.TimeCreated,
                        LastUpdated = x.LastUpdated,
                        Groups = x.RoomMembers.Where(x => x.RoomUu.Type == 2 && x.InRoom == 1)
                            .Select(x => new MemberGetListResp.GroupDTO
                            {
                                Uuid = x.RoomUuid,
                                Name = x.RoomUu.RoomName ?? "",
                                Avatar = x.RoomUu.Avatar ?? "",
                                MemberCount = x.RoomUu.RoomMembers.Count(x => x.InRoom == 1),
                            })
                            .ToList(),
                        RoleId = x.RoleId,
                        ActiveState = x.ActiveState,
                    })
                    .ToListAsync();

                var totalCount = await iQueryable.CountAsync();
                var totalPage = totalCount % request.PageSize == 0
                    ? totalCount / request.PageSize
                    : totalCount / request.PageSize + 1;

                response.Items = results;
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
        /// Thay đổi role
        /// </summary>
        /// <param name="request"></param>
        /// <remarks>
        ///     - RoleId:
        ///         + 0: user
        ///         + 1: leader
        /// </remarks>
        /// <returns></returns>
        [HttpPut("{uuid}/role")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponse), description: "Change role account Response")]
        public async Task<IActionResult> ChangeRoleId([FromRoute] string uuid, [FromBody] MemberChangeRoleRequest request)
        {
            var response = new BaseResponse();

            if (request.RoleId != 0 && request.RoleId != 1)
            {
                response.error.SetErrorCode(ErrorCode.ROLE_ID_BAD_RQ);
                return new OkObjectResult(response);
            }    

            try
            {
                var account = _dbContext.Account.Where(x => x.Uuid == uuid).SingleOrDefault();

                //Thăng và hạ cấp trong nhóm mà tk đang tham gia
                var roomMembers = _dbContext.RoomMembers
                    .Where(x => x.UserName == account.UserName)
                    .Where(x => x.InRoom == 1)
                    .ToList();

                if(roomMembers != null)
                {
                    foreach (var roomMember in roomMembers)
                    {
                        roomMember.RoleId = (sbyte)(request.RoleId == 1 ? 1 : 3);
                    }

                    _dbContext.RoomMembers.UpdateRange(roomMembers);
                }

                if (account != null)
                {
                    account.RoleId = request.RoleId;
                    _dbContext.Account.Update(account);
                }

                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// Khoá/Mở khoá tài khoản
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        [HttpPut("{uuid}/actice-state")]
        [Authorize]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponse), description: "Members Response")]
        public async Task<IActionResult> ChangeActiceState([FromRoute] string uuid)
        {
            var resp = new BaseResponse();
            var accUser = User.GetUserName();

            try
            {
                var acc = await _dbContext.Account.FirstOrDefaultAsync(x => x.Uuid == uuid);

                if (acc == null)
                {
                    resp.error.SetErrorCode(ErrorCode.ACCOUNT_NF);
                    return new OkObjectResult(resp);
                }

                if (acc.UserName == accUser)
                {
                    resp.error.SetErrorCode(ErrorCode.ACC_IS_MINE);
                    return new OkObjectResult(resp);
                }

                acc.LastUpdated = DateTime.Now;
                if (acc.ActiveState == 1)
                    acc.ActiveState = 0;
                else
                    acc.ActiveState = 1;

                ////logout acc
                //var session = _dbContext.Session.OrderByDescending(x => x.Id).FirstOrDefault(x => x.UserName == acc.UserName);
                //if (session != null)
                //{
                //    session.Status = 1;
                //    session.LogoutTime = DateTime.Now;

                //    await _dbContext.SaveChangesAsync();
                //}

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fail: {ex}");
                resp.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return new OkObjectResult(resp);
        }

        /// <summary>
        /// Reset mật khẩu tài khoản
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        [HttpPut("{uuid}/reset-password")]
        [Authorize]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponse), description: "Members Response")]
        public async Task<IActionResult> ResetPassword([FromRoute] string uuid)
        {
            var resp = new BaseResponse();
            var accUser = User.GetUserName();

            var acc = await _dbContext.Account.FirstOrDefaultAsync(x => x.Uuid == uuid);

            if (acc == null)
            {
                resp.error.SetErrorCode(ErrorCode.ACCOUNT_NOT_FOUND);
                return new OkObjectResult(resp);
            }

            if (acc.ActiveState == 0)
            {
                resp.error.SetErrorCode(ErrorCode.USER_LOCKED);
                return new OkObjectResult(resp);
            }

            if (acc.UserName == accUser)
            {
                resp.error.SetErrorCode(ErrorCode.ACC_IS_MINE);
                return new OkObjectResult(resp);
            }

            acc.LastUpdated = DateTime.Now;

            var pass = "123456";
            var passHash = MD5Util.Encrypt($"{GlobalSettings.AppSettings.KeyHash}{pass}");

            acc.PassWord = passHash;

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fail: {ex}");
                resp.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return new OkObjectResult(resp);
        }
    }
}
