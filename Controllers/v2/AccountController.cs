using ApiBuyerMorgan.Extensions;
using ChatApp.Configuaration;
using ChatApp.Enum;
using ChatApp.Extensions;
using ChatApp.Models.Request;
using ChatApp.Models.Response;
using ChatApp.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using TWChatAppApiMaster.Databases.ChatApp;
using TWChatAppApiMaster.Extensions;
using TWChatAppApiMaster.Models.Request;
using TWChatAppApiMaster.Models.Request.Admin;
using TWChatAppApiMaster.Models.Response.Admin;
using TWChatAppApiMaster.Repositories;
using TWChatAppApiMaster.SecurityManagers;

using static Google.Apis.Requests.BatchRequest;

namespace TWChatAppApiMaster.Controllers.v2
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")]
    public class AccountController : ControllerBase
    {
        private readonly DBContext _context;
        private readonly ILogger<AccountController> _logger;
        private readonly IJwtAuthenticationManager _jwtAuthenticationManager;
        private readonly ISessionRepository _sessionRepository;

        public AccountController(DBContext context, ILogger<AccountController> logger, IJwtAuthenticationManager jwtAuthenticationManager, ISessionRepository sessionRepository)
        {
            _context = context;
            _logger = logger;
            _jwtAuthenticationManager = jwtAuthenticationManager;
            _sessionRepository = sessionRepository;
        }

        /// <summary>
        /// Đăng nhập
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("login")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessage<LogInResp>), description: "LogIn Response")]
        public async Task<IActionResult> LogIn([FromBody] LogInRequest request)
        {
            var response = new BaseResponseMessage<LogInResp>();
            response.Data = new LogInResp();

            request.UserName = request.UserName.Trim().ToLower();

            try
            {
                var acc = _context.Account.Where(x => x.UserName == request.UserName && x.PassWord == request.Password && x.RoleId == 2).SingleOrDefault();

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

                        response.Data.SessionUuid = _token.SessionUuid;
                        response.Data.UserName = acc.UserName;
                        response.Data.FullName = acc.FullName;
                        response.Data.Email = acc.Email;
                        response.Data.Token = _token.AccessToken;
                        response.Data.Uuid = acc.Uuid;
                        response.Data.RoleId = acc.RoleId;
                        response.Data.Avatar = acc.Avatar;
                        response.Data.Token = _token.AccessToken;
                        response.Data.RefreshToken = _token.RefreshToken;
                        response.Data.TimeStart = _token.TimeStart ?? DateTime.UtcNow;
                        response.Data.TimeExpiredRefresh = _token.TimeExpiredRefresh ?? DateTime.UtcNow.AddMinutes(GlobalSettings.AppSettings.TokenSettings.RefreshTokenExpirationTime);
                        response.Data.TimeExpired = _token.TimeExpired ?? DateTime.UtcNow.AddMinutes(GlobalSettings.AppSettings.TokenSettings.TokenValidityTime);

                        var newSession = new Session()
                        {
                            Uuid = _token.SessionUuid,
                            DeviceId = request.DeviceId,
                            UserName = acc.UserName,
                            LoginTime = DateTime.Now,
                            Ip = request.Ip,
                            FcmToken = request.FcmToken,
                            Status = 0,
                            AccessToken = _token.AccessToken,
                            RefreshToken = _token.RefreshToken,
                            TimeExpired = response.Data.TimeExpired,
                            TimeExpiredRefresh = response.Data.TimeExpiredRefresh
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

            return new OkObjectResult(response);
        }

        /// <summary>
        /// Đăng xuất
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("logout")]
        [Authorize]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponse), description: "LogIn Response")]
        public async Task<IActionResult> LogOut([FromBody] UserBaseRequest request)
        {
            var token = HttpContext.GetToken();
            if (token is null)
            {
                return Unauthorized();
            }

            var tokenResult = TokenStore.GetByToken(token);
            if (tokenResult is null)
            {
                return Unauthorized();
            }

            var response = new BaseResponse();

            try
            {
                var session = await _context.Session.OrderByDescending(x => x.Id).FirstOrDefaultAsync(x => x.Uuid == tokenResult.SessionUuid);

                if (session != null)
                {
                    session.Status = 1;
                    session.LogoutTime = DateTime.Now;

                    await _context.SaveChangesAsync();
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
        /// Đổi mật khẩu đang nhập người dùng
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("change-password")]
        [Authorize]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponse), description: "ChangePassword Response")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var response = new BaseResponse();
            var accUser = User.GetUserName();

            try
            {
                var acc = _context.Account.Where(x => x.UserName == User.GetUserName()).SingleOrDefault();

                if (acc != null && acc.ActiveState == 1)
                {
                    if (acc.PassWord == request.OldPass)
                    {
                        acc.PassWord = request.NewPass;
                        _context.SaveChanges();
                    }
                    else
                    {
                        response.error.SetErrorCode(ErrorCode.OLD_PASS_NOT_VALID);
                    }
                }
                else
                {
                    response.error.SetErrorCode(ErrorCode.USER_LOCKED);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);

                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return Ok(response);
        }

        /// <summary>
        /// Thay đổi Avatar
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("update-avatar")]
        [Authorize]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponse), description: "Update Avatar Response")]
        public async Task<IActionResult> UpdateAvatar([FromBody] ChangeAvatarAccountRequest request)
        {
            var response = new BaseResponse();

            try
            {
                var acc = _context.Account.Where(x => x.UserName == User.GetUserName()).SingleOrDefault();

                if (acc is null)
                {
                    response.error.SetErrorCode(ErrorCode.NOT_FOUND);
                }
                else
                {
                    acc.Avatar = request.AvatarPath;
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);

                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return Ok(response);
        }

        /// <summary>
        /// Thay đổi tên hiện thị
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("update-fullname")]
        [Authorize]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponse), description: "Update FullName Response")]
        public async Task<IActionResult> UpdateFullName([FromBody] UpdateFullNameRequest request)
        {
            var response = new BaseResponse();

            try
            {
                var acc = _context.Account.Where(x => x.UserName == User.GetUserName()).SingleOrDefault();

                if (acc is null)
                {
                    response.error.SetErrorCode(ErrorCode.NOT_FOUND);
                }
                else
                {
                    acc.FullName = request.FullName;

                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);

                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return Ok(response);
        }

        /// <summary>
        /// Refresh Token
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> Refresh([FromBody] TokenRefreshRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = new BaseResponseMessage<TokenResult>();

            try
            {
                var session = await _sessionRepository.GetSessionByRefreshToken(request.RefreshToken);
                if (session is null)
                {
                    return Unauthorized();
                }

                var tokenResult = await _jwtAuthenticationManager.RefreshTokenAsync(session.AccessToken);
                if (!tokenResult.IsSuccess)
                {
                    return Unauthorized();
                }

                session.TimeExpired = tokenResult.TimeExpired;
                session.AccessToken = tokenResult.AccessToken;
                session.RefreshToken = tokenResult.RefreshToken;
                session.TimeExpiredRefresh = tokenResult.TimeExpiredRefresh;

                await _context.SaveChangesAsync();
                response.Data = tokenResult;
                response.Data.SessionUuid = session.Uuid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// Danh sách tài khoản
        /// </summary>
        /// <param name="request"></param>
        /// <remarks>
        ///     - Tất cả: isOnline = null
        ///     - Online: isOnline = true
        ///     - Offline: isOnline = false
        /// </remarks>
        /// <returns></returns>
        [HttpPost("list")]
        [Authorize]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessagePage<AccountGetListResp>), description: "Members Response")]
        public async Task<IActionResult> GetList([FromBody] AccountGetListRequest request)
        {
            var response = new BaseResponseMessagePage<AccountGetListResp>();

            try
            {
                var iQueryable = _context.Account
                    .AsNoTracking()
                    .Where(x => x.RoleId.HasValue && x.RoleId == 2)
                    .Where(x => !request.IsOnline.HasValue
                        || x.Session.Any(x => x.IsOnline == 1) == request.IsOnline)
                    //|| accOnlineList.Contains(x.UserName) == request.IsOnline)
                    .Where(x => string.IsNullOrEmpty(request.Keyword)
                        || EF.Functions.Like(x.UserName, $"%{request.Keyword.Trim()}%")
                        || EF.Functions.Like(x.FullName ?? "", $"%{request.Keyword.Trim()}%")
                        || EF.Functions.Like(x.Email ?? "", $"%{request.Keyword.Trim()}%"))
                    .OrderByDescending(x => x.RoleId)
                        .ThenByDescending(x => x.Session.Any(x => x.IsOnline == 1))
                        .ThenByDescending(x => x.Id);

                var results = await iQueryable
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(x => new AccountGetListResp()
                    {
                        Id = x.Id,
                        Uuid = x.Uuid,
                        UserName = x.UserName,
                        FullName = x.FullName ?? "",
                        Email = x.Email ?? "",
                        IsOnline = x.Session.Any(x => x.IsOnline == 1),
                        //IsOnline = accOnlineList.Contains(x.UserName),
                        LastLogin = x.Session.OrderByDescending(x => x.Id).First().LoginTime,
                        RoleId = x.RoleId!.Value,
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
        /// Xoá tài khoản
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        [HttpDelete("{uuid}")]
        [Authorize]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponse), description: "Members Response")]
        public async Task<IActionResult> Delete([FromRoute] string uuid)
        {
            var resp = new BaseResponse();
            var accUser = User.GetUserName();

            var acc = await _context.Account.FirstOrDefaultAsync(x => x.Uuid == uuid);

            if (acc == null)
            {
                resp.error.SetErrorCode(ErrorCode.ACCOUNT_NOT_FOUND);
                return new OkObjectResult(resp);
            }

            if (acc.UserName == accUser)
            {
                resp.error.SetErrorCode(ErrorCode.ACC_IS_MINE);
                return new OkObjectResult(resp);
            }

            acc.IsEnable = 0;
            acc.LastUpdated = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fail: {ex}");
                resp.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return new OkObjectResult(resp);
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

            var acc = await _context.Account.FirstOrDefaultAsync(x => x.Uuid == uuid);

            if (acc == null)
            {
                resp.error.SetErrorCode(ErrorCode.ACCOUNT_NOT_FOUND);
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

            try
            {
                await _context.SaveChangesAsync();
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

            var acc = await _context.Account.FirstOrDefaultAsync(x => x.Uuid == uuid);

            if (acc == null)
            {
                resp.error.SetErrorCode(ErrorCode.ACCOUNT_NF);
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

            //var pass = "123456";
            //var passHash = MD5Util.Encrypt($"{GlobalSettings.AppSettings.KeyHash}{pass}");
            var pass = "123456";
            var passHash = MD5Util.Encrypt($"{GlobalSettings.AppSettings.KeyHash}{pass}");

            await _context.Account.AddAsync(acc);

            

            acc.PassWord = passHash;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fail: {ex}");
                resp.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return new OkObjectResult(resp);
        }

        /// <summary>
        /// Chi tiết tài khoản
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        [HttpGet("{uuid}")]
        [Authorize]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessage<AccountGetListResp>), description: "Members Response")]
        public async Task<IActionResult> GetByUuid([FromRoute] string uuid)
        {
            var resp = new BaseResponseMessage<AccountGetListResp>();
            var accUser = User.GetUserName();

            var acc = await _context.Account.FirstOrDefaultAsync(x => x.Uuid == uuid);

            if (acc == null)
            {
                resp.error.SetErrorCode(ErrorCode.ACCOUNT_NOT_FOUND);
                return new OkObjectResult(resp);
            }

            resp.Data = new()
            {
                Id = acc.Id,
                UserName = acc.UserName,
                FullName = acc.FullName ?? "",
                Email = acc.Email ?? "",
                RoleId = acc.RoleId!.Value,
                Uuid = acc.Uuid,
                ActiveState = acc.ActiveState,
            };

            return new OkObjectResult(resp);
        }

        ///// <summary>
        ///// Chỉnh sửa tài khoản
        ///// </summary>
        ///// <param name="uuid"></param>
        ///// <param name="request"></param>
        ///// <returns></returns>
        //[HttpPut("{uuid}")]
        //[Authorize]
        //[SwaggerResponse(statusCode: 200, type: typeof(BaseResponse), description: "Members Response")]
        //public async Task<IActionResult> Update([FromRoute] string uuid, [FromBody] AccountUpdateRequest request)
        //{
        //    var resp = new BaseResponseMessage<AccountGetListResp>();
        //    var accUser = User.GetUserName();

        //    if (request.RoleId != 2)
        //    {
        //        resp.error.SetErrorCode(ErrorCode.ROLE_ID_BAD_RQ);
        //        return new OkObjectResult(resp);
        //    }    

        //    var acc = await _context.Account.FirstOrDefaultAsync(x => x.Uuid == uuid);

        //    if (acc == null)
        //    {
        //        resp.error.SetErrorCode(ErrorCode.ACCOUNT_NOT_FOUND);
        //        return new OkObjectResult(resp);
        //    }

        //    if (acc.UserName == accUser)
        //    {
        //        resp.error.SetErrorCode(ErrorCode.ACC_IS_MINE);
        //        return new OkObjectResult(resp);
        //    }

        //    acc.LastUpdated = DateTime.Now;
        //    acc.FullName = request.FullName;
        //    acc.RoleId = request.RoleId;    

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Fail: {ex}");
        //        resp.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
        //    }

        //    return new OkObjectResult(resp);
        //}

        /// <summary>
        /// Thêm tài khoản
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("add")]
        [Authorize]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponse), description: "Members Response")]
        public async Task<IActionResult> Add([FromBody] AccountAddRequest request)
        {
            var resp = new BaseResponse();
            var accUser = User.GetUserName();

            if (string.IsNullOrEmpty(request.UserName))
            {
                resp.error.SetErrorCode(ErrorCode.USER_NAME_RQ);
                return new OkObjectResult(resp);
            }

            if (string.IsNullOrEmpty(request.Email))
            {
                resp.error.SetErrorCode(ErrorCode.EMAIL_RQ);
                return new OkObjectResult(resp);
            }

            if (request.RoleId != 2)
            {
                resp.error.SetErrorCode(ErrorCode.ROLE_ID_BAD_RQ);
                return new OkObjectResult(resp);
            }

            var accList = await _context.Account.Where(x => x.UserName == request.UserName || x.Email == request.Email).ToListAsync();

            if (accList.Exists(x => x.UserName.ToLower().Trim() == request.UserName.ToLower().Trim()))
            {
                resp.error.SetErrorCode(ErrorCode.EXISTS);
                return new OkObjectResult(resp);
            }

            if (accList.Exists(x => x.Email.ToLower().Trim() == request.Email.ToLower().Trim()))
            {
                resp.error.SetErrorCode(ErrorCode.EMAIL_EXISTS);
                return new OkObjectResult(resp);
            }

            var pass = "123345";
            var passHash = MD5Util.Encrypt($"{GlobalSettings.AppSettings.KeyHash}{pass}");

            var acc = new Account()
            {
                Uuid = Guid.NewGuid().ToString(),
                Email = request.Email,
                UserName = request.UserName,
                PassWord = passHash,
                Status = 1,
                RoleId = request.RoleId,
                FullName = request.FullName,
                ActiveState = 1,
            };
            await _context.Account.AddAsync(acc);

            try
            {
                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError($"Fail: {ex}");
                resp.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
                resp.error.Message = !string.IsNullOrEmpty(SupportExtension.GetExceptionMessages(ex, "")) ? SupportExtension.GetExceptionMessages(ex, "") : resp.error.Message;
                if (resp.error.Message.Contains("Duplicate entry") && resp.error.Message.Contains("for key 'user_name'"))
                {
                    resp.error.SetErrorCode(ErrorCode.EXISTS);
                    return new OkObjectResult(resp);
                }
            }

            return new OkObjectResult(resp);
        }

        

        /// <summary>
        /// Quên mật khẩu bước 2: Xác minh code và email
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("forgot/stage-2")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponse), description: "Members Response")]
        public async Task<IActionResult> ForgotStage2([FromBody] ForgotEmailStage2Request request)
        {
            var resp = new BaseResponse();

            try
            {
                var check = await _context.Account.AsNoTracking().FirstOrDefaultAsync(x => x.Email == request.Email && x.IsEnable == 1);
                if (check == null)
                {
                    resp.error.SetErrorCode(ErrorCode.NOT_FOUND);
                    resp.error.Message = "Email không tồn tại";
                    return new OkObjectResult(resp);
                }

                
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fail: {ex}");
                resp.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return new OkObjectResult(resp);
        }

        /// <summary>
        /// Quên mật khẩu bước 3: Đặt lại mật khẩu
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("forgot/stage-3")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponse), description: "Members Response")]
        public async Task<IActionResult> ForgotStage3([FromBody] ForgotEmailStage3Request request)
        {
            var resp = new BaseResponse();

            try
            {
                var account = await _context.Account.FirstOrDefaultAsync(x => x.Email == request.Email && x.IsEnable == 1);
                if (account == null)
                {
                    resp.error.SetErrorCode(ErrorCode.NOT_FOUND);
                    resp.error.Message = "Email không tồn tại";
                    return new OkObjectResult(resp);
                }

                account.PassWord = request.NewPassword;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fail: {ex}");
                resp.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return new OkObjectResult(resp);
        }

        /// <summary>
        /// Login client
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("login-client")]
        [Authorize]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessage<LogInResp>), description: "Đăng nhập vào client")]
        public async Task<IActionResult> LoginClient([FromBody] LoginWithLeader request)
        {
            var response = new BaseResponseMessage<LogInResp>();

            try
            {
                var acc = await _context.Account.FirstOrDefaultAsync(x => x.UserName == request.UserName && x.IsEnable == 1 && x.RoleId != 2);
                if (acc == null)
                {
                    response.error.SetErrorCode(ErrorCode.USER_LOCKED_OR_NF);
                    return new OkObjectResult(response);
                }

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
                    response.Data = new LogInResp();
                    response.Data.Token = _token.AccessToken;

                    var newSession = new Session()
                    {
                        Uuid = Guid.NewGuid().ToString(),
                        DeviceId = request.DeviceId,
                        UserName = acc.UserName,
                        LoginTime = DateTime.Now,
                        Ip = request.Ip,
                        FcmToken = request.FcmToken,
                        Status = 0,
                        AccessToken = _token.AccessToken,
                        RefreshToken = _token.RefreshToken,
                        TimeExpiredRefresh = _token.TimeExpiredRefresh ?? DateTime.UtcNow.AddMinutes(GlobalSettings.AppSettings.TokenSettings.RefreshTokenExpirationTime),
                        TimeExpired = _token.TimeExpired ?? DateTime.UtcNow.AddMinutes(GlobalSettings.AppSettings.TokenSettings.TokenValidityTime),
                };

                    _context.Session.Add(newSession);
                    _context.SaveChanges();

                    //var newDevice = _context.Devices.Where(x => x.UserName == acc.UserName && x.DeviceId == request.DeviceId).FirstOrDefault();

                    //if (newDevice is null)
                    //{
                    //    newDevice = new Devices()
                    //    {
                    //        Os = request.Os ?? "",
                    //        DeviceId = request.DeviceId ?? "",
                    //        DeviceName = request.DeviceName ?? "",
                    //        UserName = acc.UserName,
                    //        LastUsed = DateTime.Now,
                    //        Ip = request.Ip,
                    //        Address = request.Address,
                    //    };

                    //    _context.Devices.Add(newDevice);
                    //}
                    //else
                    //{
                    //    newDevice.Ip = request.Ip;
                    //    newDevice.Address = request.Address;
                    //    newDevice.LastUsed = DateTime.Now;
                    //    newDevice.Status = 1;
                    //}

                    //_context.SaveChanges();

                    _logger.LogInformation($"Call login user {acc.UserName} from admin");
                }
                else
                {
                    response.error.SetErrorCode(ErrorCode.USER_LOCKED);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fail: {ex}");
                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return new OkObjectResult(response);
        }
    }
}
