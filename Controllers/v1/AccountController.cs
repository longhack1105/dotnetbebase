using ApiBuyerMorgan.Extensions;
using ChatApp.Configuaration;
using ChatApp.Enum;
using ChatApp.Extensions;
using ChatApp.Models.DataInfo;
using ChatApp.Models.Request;
using ChatApp.Models.Response;
using ChatApp.Socket;
using ChatApp.Utils;
using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Services.DelegatedAuthorization;
using Microsoft.VisualStudio.Services.Users;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Threading.Channels;
using TWChatAppApiMaster.Databases.ChatApp;
using TWChatAppApiMaster.Extensions;
using TWChatAppApiMaster.Models;
using TWChatAppApiMaster.Models.Request;
using TWChatAppApiMaster.Models.Response;
using TWChatAppApiMaster.Repositories;
using TWChatAppApiMaster.SecurityManagers;
using TWChatAppApiMaster.Socket;
using TWChatAppApiMaster.Utils;
using static ChatApp.Socket.ChatHandler;
using static Google.Apis.Requests.BatchRequest;

namespace ChatApp.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
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
                var acc = _context.Account
                    .Include(x => x.Session)
                    .Where(x => x.UserName == request.UserName && x.PassWord == request.Password && x.RoleId != 2).SingleOrDefault();

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

                        if (string.IsNullOrEmpty(request.FcmToken) || request.FcmToken.Length < 20)
                            request.FcmToken = null;

                        response.Data.TokenFcm = !string.IsNullOrEmpty(request.FcmToken) ? request.FcmToken : acc?.Session?.OrderByDescending(x => x.Id)?.FirstOrDefault()?.FcmToken;

                        var newSession = new Session()
                        {
                            Uuid = _token.SessionUuid,
                            DeviceId = request.DeviceId,
                            UserName = acc.UserName,
                            LoginTime = DateTime.Now,
                            Ip = request.Ip,
                            FcmToken = response.Data.TokenFcm,
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

            return Ok(response);
        }

        /// <summary>
        /// Đăng nhập với access token
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("login-with-token")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessage<LogInResp>), description: "LogIn with token")]
        public async Task<IActionResult> LogInWithToken([FromBody] LogInWithTokenRequest request)
        {
            var response = new BaseResponseMessage<LogInResp>();
            response.Data = new LogInResp();

            //request.UserName = request.UserName.Trim().ToLower();

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var _tokenData = tokenHandler.ReadJwtToken(request.AccessToken);

                if (_tokenData is null)
                {
                    response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
                    return new OkObjectResult(response);
                }

                var accountUuid = _tokenData.Claims.FirstOrDefault(claim => claim.Type == "AccountUuid")?.Value ?? null;

                var acc = _context.Account
                .Include(x => x.Session)
                .Where(x => x.Uuid == accountUuid && x.RoleId != 2).SingleOrDefault();

                if (acc is null)
                {
                    response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
                    return new OkObjectResult(response);
                }

                var sessionDb = acc.Session
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefault();

                if (sessionDb is null)
                {
                    response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
                    return new OkObjectResult(response);
                }

                var _token = new TokenModel
                {
                    AccessToken = request.AccessToken,
                    RefreshToken = sessionDb.RefreshToken,
                    SessionUuid = sessionDb.Uuid,
                    TimeExpired = sessionDb.TimeExpired,
                    TimeExpiredRefresh = sessionDb.TimeExpiredRefresh
                };

                if (acc != null)
                {
                    if (acc.ActiveState == 1)
                    {
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

                        if (string.IsNullOrEmpty(request.FcmToken) || request.FcmToken.Length < 20)
                            request.FcmToken = null;

                        response.Data.TokenFcm = !string.IsNullOrEmpty(request.FcmToken) ? request.FcmToken : acc?.Session?.OrderByDescending(x => x.Id)?.FirstOrDefault()?.FcmToken;

                        //var newSession = new Session()
                        //{
                        //    Uuid = _token.SessionUuid,
                        //    DeviceId = request.DeviceId,
                        //    UserName = acc.UserName,
                        //    LoginTime = DateTime.Now,
                        //    Ip = request.Ip,
                        //    FcmToken = response.Data.TokenFcm,
                        //    Status = 0,
                        //    AccessToken = _token.AccessToken,
                        //    RefreshToken = _token.RefreshToken,
                        //    TimeExpired = response.Data.TimeExpired,
                        //    TimeExpiredRefresh = response.Data.TimeExpiredRefresh
                        //};

                        sessionDb.DeviceId = request.DeviceId;
                        sessionDb.UserName = acc.UserName;
                        sessionDb.LoginTime = DateTime.Now;
                        sessionDb.Ip = request.Ip;
                        sessionDb.FcmToken = response.Data.TokenFcm;
                        sessionDb.Status = 0;
                        sessionDb.AccessToken = _token.AccessToken;
                        sessionDb.RefreshToken = _token.RefreshToken;
                        sessionDb.TimeExpired = response.Data.TimeExpired;
                        sessionDb.TimeExpiredRefresh = response.Data.TimeExpiredRefresh;

                        _context.Session.Update(sessionDb);

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

            var response = new BaseResponse();

            try
            {
                var session = _context.Session.OrderByDescending(x => x.Id).FirstOrDefault(x => x.AccessToken == token);

                if (session != null)
                {
                    if (session.DeviceId != null) // nếu không phải là web thì logout hết sesion cùng DeviceId
                    {
                        //logout all session with same device as this session
                        var sessions = _context.Session.AsNoTracking()
                            .Where(x => x.UserName == session.UserName)
                            .Where(x => x.Status == 0)
                            .Where(x => x.DeviceId == session.DeviceId && x.DeviceId != null)
                            .ToList();

                        foreach (var item in sessions)
                        {
                            item.Status = 1;
                            item.LogoutTime = DateTime.Now;
                            item.FcmToken = null;
                        }

                        _context.Session.UpdateRange(sessions);
                    }

                    session.Status = 1;
                    session.LogoutTime = DateTime.Now;
                    session.FcmToken = null;
                    _context.Session.Update(session);

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);

                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return Ok(response);
        }

        [HttpPost("register")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessage<LogInResp>), description: "Register Response")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var response = new BaseResponseMessage<LogInResp>();
            response.Data = new LogInResp();

            try
            {
                request.UserName = request.UserName.Trim().ToLower();
                request.Email = request.Email.Trim().ToLower();

                var validCharacters = "+0123456789";

                var arrUserChars = request.UserName.ToCharArray();

                // Kiểm tra ký tự hợp lệ
                foreach (var item in arrUserChars)
                {
                    if (!validCharacters.Contains(item))
                    {
                        response.error.SetErrorCode(ErrorCode.INVALID_PARAM);
                        return Ok(response);
                    }
                }

                // Kiểm tra request có dữ liệu hay không
                if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.FullName))
                {
                    response.error.SetErrorCode(ErrorCode.INVALID_PARAM);
                    return Ok(response);
                }

                // kiểm tra otp
                var checkOtp = await CheckOtp(request.UserName, request.OtpCode, (int)EnumSenderAction.REGISTER);

                if (checkOtp?.error?.Code != ErrorCode.SUCCESS)
                {
                    response.error.SetErrorCode(ErrorCode.OTP_INVALID);
                    response.error.Message = checkOtp?.error?.Message;
                    return Ok(response);
                }

                // Kiểm tra tài khoản có tồn tại hay không
                var acc = _context.Account.Where(x => x.UserName == request.UserName).SingleOrDefault();

                if (acc != null)
                {
                    response.error.SetErrorCode(ErrorCode.EXISTS);
                    return Ok(response);
                }

                // Đăng ký
                acc = new Account()
                {
                    Uuid = Guid.NewGuid().ToString(),
                    Email = request.Email,
                    UserName = request.UserName,
                    PassWord = request.Password == null ? "109889f941630d269546335f728f3558" : request.Password,
                    Status = 1,
                    RoleId = 1,
                    FullName = request.FullName,
                    ActiveState = 1,
                };

                _context.Account.Add(acc);
                _context.SaveChanges();

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

                if (string.IsNullOrEmpty(request.FcmToken) || request.FcmToken.Length < 20)
                    request.FcmToken = null;

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
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);

                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return Ok(response);
        }

        /// <summary>
        /// Thay đổi mật khẩu đang nhập người dùng
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("forget-password")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponse), description: "ResetPassword Response")]
        public async Task<IActionResult> ForgetPassword([FromBody] ResetPasswordRequest request)
        {
            request.UserName = request.UserName.Trim().ToLower();
            var response = new BaseResponse();

            try
            {
                // Kiểm tra otp
                if (string.IsNullOrEmpty(request.OtpCode))
                {
                    response.error.SetErrorCode(ErrorCode.OTP_INVALID);
                    return Ok(response);
                }

                // kiểm tra otp
                var checkOtp = await CheckOtp(request.UserName, request.OtpCode, (int)EnumSenderAction.FOGOT_PASSWORD);

                if (checkOtp?.error?.Code != ErrorCode.SUCCESS)
                {
                    response.error.SetErrorCode(ErrorCode.OTP_INVALID);
                    response.error.Message = checkOtp?.error?.Message;
                    return Ok(response);
                }

                var acc = _context.Account.AsNoTracking()
                    .Where(x => x.UserName == request.UserName && x.ActiveState == 1)
                    .SingleOrDefault();

                // Kiểm tra tài khoản
                if (acc == null)
                {
                    response.error.SetErrorCode(ErrorCode.ACCOUNT_NF);
                    return Ok(response);
                }



                //var checkOtp = await CheckOtp(request.UserName, request.OtpCode, (int)EnumSenderAction.FOGOT_PASSWORD);

                //if (!checkOtp)
                //{
                //    response.error.SetErrorCode(ErrorCode.OTP_INVALID);
                //    return Ok(response);
                //}

                // Đổi mật khẩu
                acc.PassWord = request.NewPass;
                _context.Account.Update(acc);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);

                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return Ok(response);
        }

        /// <summary>
        /// Reset mật khẩu (mặc định 123456)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("reset-password")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponse), description: "ResetPassword Response")]
        public async Task<IActionResult> ResetPassword([FromBody] GetOtpRequest request)
        {
            var response = new BaseResponse();

            try
            {
                var acc = await _context.Account.Where(x => x.UserName == request.PhoneNumber).FirstOrDefaultAsync();
                if (acc != null && acc.ActiveState == 1)
                {
                    var pass = "123456";
                    var passHash = MD5Util.Encrypt($"{GlobalSettings.AppSettings.KeyHash}{pass}");

                    acc.PassWord = passHash;
                    acc.LastUpdated = DateTime.Now;
                    _context.SaveChanges();
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
        /// Lấy mã xác thực (Action: 1-Register, 2-Fogot password, 3-Change password)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("request-otp")]
        //[Authorize]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessage<DateTime>), description: "Request Otp Response")]
        public async Task<IActionResult> RequestOtp([FromBody] GetOtpRequest request)
        {
            var response = new BaseResponseMessage<DateTime>();

            request.PhoneNumber = request.PhoneNumber.Trim();

            try
            {
                var acc = _context.Account.Where(x => x.UserName.ToLower() == request.PhoneNumber.ToLower()).SingleOrDefault();
                if (request.Action == (int)EnumSenderAction.REGISTER)
                {
                    if (acc != null)
                    {
                        response.error.SetErrorCode(ErrorCode.EXISTS);
                        return Ok(response);
                    }
                }
                else
                {
                    if (acc == null)
                    {
                        response.error.SetErrorCode(ErrorCode.ACCOUNT_NF);
                        return Ok(response);
                    }
                }

                var sendOtpResult = await DoSendOTP(request.PhoneNumber, request.Action); //EnumSenderAction

                if (sendOtpResult?.error?.Code != ErrorCode.SUCCESS)
                {
                    response.error.SetErrorCode(ErrorCode.GEN_TOKEN_FAILED);
                    response.error.Message = sendOtpResult.error.Message;
                    return Ok(response);
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
        /// Lấy otp khi đăng ký
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("request-register-otp")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessage<DateTime>), description: "Register Request Otp Response")]
        public async Task<IActionResult> RequestRegisterOtp([FromBody] GetOtpRequest request)
        {
            var response = new BaseResponseMessage<DateTime>();

            request.PhoneNumber = request.PhoneNumber.Trim();

            try
            {
                var acc = _context.Account.Where(x => x.UserName.ToLower() == request.PhoneNumber.ToLower()).SingleOrDefault();

                if (acc != null)
                {
                    response.error.SetErrorCode(ErrorCode.EXISTS);
                    return Ok(response);
                }

                var sendOtpResult = await DoSendOTP(request.PhoneNumber, (int)EnumSenderAction.REGISTER);

                if (sendOtpResult?.error?.Code != ErrorCode.SUCCESS)
                {
                    response.error.SetErrorCode(ErrorCode.GEN_TOKEN_FAILED);
                    response.error.Message = sendOtpResult.error.Message;
                    return Ok(response);
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
        /// Kiểm tra Otp
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("check-otp")]
        //[Authorize]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessage<DateTime>), description: "Check Otp Response")]
        public async Task<IActionResult> CheckOtp([FromBody] GetOtpRequest request)
        {
            var response = new BaseResponseMessage<DateTime>();

            request.PhoneNumber = request.PhoneNumber.Trim();

            try
            {
                //var acc = _context.Account.Where(x => x.UserName.ToLower() == request.PhoneNumber.ToLower()).SingleOrDefault();
                //if (request.Action == (int)EnumSenderAction.REGISTER)
                //{
                //    if (acc != null)
                //    {
                //        response.error.SetErrorCode(ErrorCode.EXISTS);
                //        return Ok(response);
                //    }
                //}
                //else
                //{
                //    if (acc == null)
                //    {
                //        response.error.SetErrorCode(ErrorCode.ACCOUNT_NF);
                //        return Ok(response);
                //    }
                //}


                //var checkOtp = await CheckOtp(request.PhoneNumber, request.Otp, request.Action); //EnumSenderAction

                //if (!checkOtp)
                //{
                //    response.error.SetErrorCode(ErrorCode.OTP_INVALID);
                //    return Ok(response);
                //}

                // kiểm tra otp
                var checkOtp = await CheckOtp(request.PhoneNumber, request.Otp, request.Action);

                if (checkOtp?.error?.Code != ErrorCode.SUCCESS)
                {
                    response.error.SetErrorCode(ErrorCode.OTP_INVALID);
                    response.error.Message = checkOtp?.error?.Message;
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);

                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return Ok(response);
        }

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

                    var roomAvailable = await MessengeExtension.GetRoomAvailable(_context, acc.UserName, true);
                    var lstUserToNotify = roomAvailable
                        .Include(x => x.RoomMembers)
                        .SelectMany(x => x.RoomMembers.Select(rm => rm.UserName))
                        .Distinct()
                        .ToList();
                    ;

                    if (lstUserToNotify.Any())
                    {
                        var responseData = new TWChatAppApiMaster.Socket.Message
                        {
                            MsgType = (int)MessageType.TYPE_CHANGE_PROFILE,
                            Data = JsonConvert.SerializeObject(new ChangeProfileResponse
                            {
                                Uuid = acc.Uuid,
                                FullName = acc.FullName,
                                Avatar = acc.Avatar,
                                Username = acc.UserName,
                            }),
                        };

                        await ChatHandler.getInstance().SendMessageToGroupUsersAsync(JsonConvert.SerializeObject(responseData), lstUserToNotify);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);

                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return Ok(response);
        }

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

                    var roomAvailable = await MessengeExtension.GetRoomAvailable(_context, acc.UserName, true);
                    var lstUserToNotify = roomAvailable
                        .Include(x => x.RoomMembers)
                        .SelectMany(x => x.RoomMembers.Select(rm => rm.UserName))
                        .Distinct()
                        .ToList();
                    ;

                    if(lstUserToNotify.Any())
                    {
                        var responseData = new TWChatAppApiMaster.Socket.Message
                        {
                            MsgType = (int)MessageType.TYPE_CHANGE_PROFILE,
                            Data = JsonConvert.SerializeObject(new ChangeProfileResponse
                            {
                                Uuid = acc.Uuid,
                                FullName = acc.FullName,
                                Avatar = acc.Avatar,
                                Username = acc.UserName,
                            }),
                        };

                        await ChatHandler.getInstance().SendMessageToGroupUsersAsync(JsonConvert.SerializeObject(responseData), lstUserToNotify);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);

                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return Ok(response);
        }

        [HttpPost("toggle-notification-receive")]
        [Authorize]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponse), description: "toggle receive fcm notify Response")]
        public async Task<IActionResult> ToggleNotify([FromBody] RegisterNotifyStateRequest request)
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
                    acc.ReceiveNotifyStatus = (sbyte)request.state;
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

        [HttpPost("list-account")]
        [Authorize]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessageItem<AccountDTO>), description: "List account Response")]
        public async Task<IActionResult> ListAccount([FromBody] GetListAccountRequest request)
        {
            var response = new BaseResponseMessageItem<AccountDTO>();

            try
            {
                var roleId = _context.Account.Where(x => x.UserName == User.GetUserName()).Select(x => x.RoleId).ToList();
                if (roleId != null)
                {
                    if (roleId.Contains(2))
                    {
                        var lstAccount = _context.Account.Where(x => x.RoleId == request.RoleId && x.ActiveState == request.LockState)
                                                 .Where(x => EF.Functions.Like(x.FullName, $"%{request.Keyword}%"))
                                                 .Skip((request.Page - 1) * request.PageSize)
                                                 .Take(request.PageSize).ToList();

                        if (lstAccount != null)
                        {
                            foreach (var item in lstAccount)
                            {
                                var frdDto = new AccountDTO
                                {
                                    Uuid = item.Uuid,
                                    RoleId = item.RoleId,
                                    UserName = item.UserName,
                                    FullName = item.FullName,
                                    Email = item.Email,
                                    ReceiveNotifyStatus = item.ReceiveNotifyStatus,
                                    LastUpdated = item.LastUpdated,
                                    TimeCreated = item.TimeCreated,
                                    Status = item.Status,
                                    Avatar = item.Avatar,
                                    ActiveState = item.ActiveState,

                                };

                                response.Items.Add(frdDto);
                            }
                        }
                    }
                    else
                    {
                        response.error.SetErrorCode(ErrorCode.PERMISION_DENIED);
                    }
                }
                else
                {
                    response.error.SetErrorCode(ErrorCode.NOT_FOUND);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);

                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
                response.error.Message = ex.Message;
            }

            return Ok(response);
        }

        [HttpPost("toggle-lock-account")]
        [Authorize]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponse), description: "toggle lock account Response")]
        public async Task<IActionResult> ToggleLockAccount([FromBody] LockAccountRequest request)
        {
            var response = new BaseResponseMessageItem<AccountDTO>();

            try
            {
                var account = _context.Account.Where(x => x.Uuid == request.Uuid).SingleOrDefault();

                if (account != null)
                {
                    account.ActiveState = (sbyte)request.ActiveState;
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

        [HttpPost("change-role-account")]
        [Authorize]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponse), description: "Change role account Response")]
        public async Task<IActionResult> ChangeRoleId([FromBody] ChangeRoleRequest request)
        {
            var response = new BaseResponse();

            try
            {
                var myInfo = _context.Account.Where(x => x.Uuid == User.GetAccountUuid()).SingleOrDefault();
                if (myInfo != null && myInfo.RoleId == 2)
                {
                    var account = _context.Account.Where(x => x.Uuid == request.Uuid).SingleOrDefault();
                    if (account != null)
                    {
                        account.RoleId = request.RoleId;
                        _context.SaveChanges();
                    }
                }
                else
                {
                    response.error.SetErrorCode(ErrorCode.PERMISION_DENIED);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);

                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return Ok(response);
        }

        [HttpPost("update-fcm-token")]
        [Authorize]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponse), description: "update fcm Response")]
        public async Task<IActionResult> UpdateFCMToken([FromBody] UpdateFCMTokenRequest request)
        {
            var response = new BaseResponse();

            try
            {
                var sessionDb = _context.Session
                    .Where(x => x.Uuid == request.Uuid)
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefault();
                if (sessionDb != null)
                {
                    sessionDb.FcmToken = request.Token;
                    _context.SaveChanges();
                }
                else
                {
                    response.error.SetErrorCode(ErrorCode.NOT_FOUND);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return new OkObjectResult(response);
        }

        ///// <summary>
        ///// test send firebase
        ///// </summary>
        ///// <param name="request"></param>
        ///// <returns></returns>
        //[HttpPost("send-msg-firebase")]
        //[Authorize]
        //[SwaggerResponse(statusCode: 200, type: typeof(BaseResponse), description: "SendMsgFirebase")]
        //public async Task<IActionResult> SendMsgFirebase([FromBody] UpdateFCMTokenRequest request)
        //{
        //    var response = new BaseResponse();

        //    try
        //    {
        //        //List<FirebaseAdmin.Messaging.Message> message = new List<FirebaseAdmin.Messaging.Message>();
        //        var message = new FirebaseAdmin.Messaging.Message()
        //        {
        //            Data = new Dictionary<string, string>()
        //            {
        //                {"myData", "detail sent in message" },
        //            },
        //            Token = request.Token,
        //            //Topic = "my topic", // only if not targeting a specific device(s)
        //            Notification = new Notification()
        //            {
        //                Title = "Test from code",
        //                Body = "Body of message is here",
        //            },
        //        };

        //        await FirebaseMessaging.DefaultInstance.SendAsync(message);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex.Message, ex);
        //        response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
        //    }

        //    return new OkObjectResult(response);
        //}

        private async Task<BaseResponse?> CheckOtp(string phone, string otp, int action)
        {
            //var result = false;
            //var checkOtp = _context.OtpPhone.Where(x => x.PhoneNumber.Equals(Phone) && x.Otp.Equals(otp) && x.Status == 0).FirstOrDefault();

            //if (checkOtp != null)
            //{
            //    checkOtp.Status = 1;
            //    checkOtp.UserUsed = accActived;
            //    checkOtp.Note = note;

            //    _context.SaveChanges();

            //    result = true;
            //}

            ////TODO: Remove late
            //if (otp.Equals("123456"))
            //{
            //    return true;
            //}

            var sendMailRes = await MailService.VerifySmsAsync(phone, otp, action);

            var otpDb = _context.OtpPhone.AsNoTracking()
                .FirstOrDefault(x => x.Otp == otp && x.PhoneNumber == phone && x.Action == action && x.Status == 1 && x.TimeExpired > DateTime.Now);

            if (sendMailRes?.error?.Code == ErrorCode.SUCCESS)
            {
                // lưu otp
                if (otpDb == null)
                {
                    var newOtp = new OtpPhone
                    {
                        PhoneNumber = phone,
                        Otp = otp,
                        Action = (sbyte)action,
                        TimeCreated = DateTime.Now,
                        TimeExpired = DateTime.Now.AddMinutes(10),
                        Status = 1,
                    };

                    _context.Add(newOtp);
                }
                else
                {
                    otpDb.Status = 1;
                    otpDb.TimeExpired = DateTime.Now.AddMinutes(10);
                    _context.Update(otpDb);
                }
            }
            else
            {
                // check lần 2 ở db
                if (otpDb != null)
                {
                    otpDb.Status = 0;
                    _context.Update(otpDb);

                    sendMailRes.error.Code = ErrorCode.SUCCESS;
                    sendMailRes.error.Message = "";
                }
            }

            _context.SaveChanges();

            return sendMailRes;
        }

        private async Task<BaseResponse?> DoSendOTP(string Phone, int Action)
        {
            var sendMailRes = await MailService.SendSmsAsync(Phone, Action);

            return sendMailRes;
        }

        /// <summary>
        /// Refresh Token
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("refresh-token")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessage<TokenResult>), description: "refresh-token")]
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
        /// Danh sách thiết bị đăng nhập
        /// </summary>
        /// <returns></returns>
        [HttpGet("devices")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessageItem<AccountGetDevicesResp>), description: "resp result")]
        public async Task<IActionResult> GetDevices()
        {
            var accUser = User.GetUserName();
            var response = new BaseResponseMessageItem<AccountGetDevicesResp>();

            try
            {
                var results = await _context.Devices
                    .AsNoTracking()
                    .Where(x => x.UserName == accUser)
                    .Where(x => x.Status == 1)
                    .Select(x => new AccountGetDevicesResp
                    {
                        DeviceId = x.DeviceId,
                        DeviceName = x.DeviceName,
                        Os = x.Os,
                        Address = x.Address,
                        Ip = x.Ip,
                        TimeLastUsed = x.LastUsed,
                    })
                    .OrderByDescending(x => x.TimeLastUsed)
                    .ToListAsync();

                response.Items = results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// Xoá thiết bị đăng nhập
        /// </summary>
        /// <returns></returns>
        [HttpDelete("devices/{deviceId}")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponse), description: "resp result")]
        public async Task<IActionResult> GetDevices([FromRoute] string deviceId)
        {
            var accUser = User.GetUserName();
            var response = new BaseResponse();

            try
            {
                var deviceDb = await _context.Devices
                    .Where(x => x.UserName == accUser)
                    .Where(x => x.Status == 1)
                    .Where(x => x.DeviceId == deviceId)
                    .FirstOrDefaultAsync();

                if (deviceDb == null)
                {
                    response.error.SetErrorCode(ErrorCode.NOT_FOUND);
                    return new OkObjectResult(response);
                }

                deviceDb.Status = 2;

                var sessions = await _context.Session
                    .Where(x => x.UserName == accUser && x.DeviceId == deviceId)
                    .Where(x => x.Status == 0)
                    .ToListAsync();

                sessions.ForEach(x => x.Status = 1);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return new OkObjectResult(response);
        }

        [HttpPost("check-username")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessage<LogInResp>), description: "Check username")]
        public async Task<IActionResult> CheckUsername([FromBody] RegisterRequest request)
        {
            var response = new BaseResponseMessage<LogInResp>();
            response.Data = new LogInResp();

            try
            {
                request.UserName = request.UserName.Trim().ToLower();
                //request.Email = request.Email.Trim().ToLower();

                var validCharacters = "+0123456789";

                var arrUserChars = request.UserName.ToCharArray();
                foreach (var item in arrUserChars)
                {
                    if (!validCharacters.Contains(item))
                    {
                        response.error.SetErrorCode(ErrorCode.INVALID_PARAM);

                        return Ok(response);
                    }
                }

                if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.FullName))
                {
                    response.error.SetErrorCode(ErrorCode.INVALID_PARAM);

                    return Ok(response);
                }
                else
                {
                    var acc = _context.Account.Where(x => x.UserName == request.UserName).SingleOrDefault();

                    if (acc != null)
                    {
                        response.error.SetErrorCode(ErrorCode.EXISTS);
                    }
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
