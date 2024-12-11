using ApiBuyerMorgan.Extensions;
using ChatApp.Enum;
using ChatApp.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Drawing;
using System.Security.Claims;
using TWChatAppApiMaster.Databases.ChatApp;
using TWChatAppApiMaster.Models.Request;
using TWChatAppApiMaster.Repositories;
using TWChatAppApiMaster.SecurityManagers;
using TWChatAppApiMaster.Utils;

namespace TWChatAppApiMaster.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class QRLoginController : ControllerBase
    {
        private readonly DBContext _context;
        private readonly ILogger<QRLoginController> _logger;
        private readonly IJwtAuthenticationManager _jwtAuthenticationManager;
        private readonly ISessionRepository _sessionRepository;

        public QRLoginController(DBContext context, ILogger<QRLoginController> logger, IJwtAuthenticationManager jwtAuthenticationManager, ISessionRepository sessionRepository)
        {
            _context = context;
            _logger = logger;
            _jwtAuthenticationManager = jwtAuthenticationManager;
            _sessionRepository = sessionRepository;
        }

        /// <summary>
        /// Tạo QR đăng nhập
        /// </summary>
        /// <returns></returns>
        [HttpPost("GenerateQRCode")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponse), description: "Response")]
        public async Task<IActionResult> GenerateQRCode([FromBody] GenerateQRLoginRequest request)
        {
            var resp = new BaseResponse();
            try
            {
                string keyQR = Guid.NewGuid().ToString();
                await _context.LoginQrCode.AddAsync(new LoginQrCode
                {
                    Uuid = keyQR,
                    Address = request.Address,
                    DeviceId = request.DeviceId,
                    DeviceName = request.DeviceName,
                    FcmToken = request.FcmToken,
                    Os = request.Os,
                    Ip = request.Ip,
                });

                await _context.SaveChangesAsync();

                QRCodeGeneratorService qrService = new QRCodeGeneratorService();
                Bitmap qrCodeImage = qrService.GenerateQRCode(keyQR);

                using (var ms = new MemoryStream())
                {
                    qrCodeImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    return File(ms.ToArray(), "image/png");
                }
            } 
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                resp.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
                return new OkObjectResult(resp);
            }
        }

        /// <summary>
        /// Kiểm tra QR đã đăng nhập hay chưa
        /// </summary>
        /// <param name="request">Client tự giải mã QR để lấy</param>
        /// <returns></returns>
        [HttpPost("poll")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessage<LogInResp>), description: "Response")]
        public async Task<IActionResult> PollQRCodeStatus([FromBody] QRScanRequest request)
        {
            var resp = new BaseResponseMessage<LogInResp>();

            try
            {
                var qrLogin = await _context.LoginQrCode
                    .AsNoTracking()
                    .Where(x => x.Uuid == request.KeyQR)
                    .FirstOrDefaultAsync();

                if (qrLogin == null)
                {
                    resp.error.SetErrorCode(ErrorCode.NOT_FOUND);
                    return new OkObjectResult(resp);
                }

                if (qrLogin.TimeExpired < DateTime.UtcNow)
                {
                    resp.error.SetErrorCode(ErrorCode.QR_EXPIRED);
                    return new OkObjectResult(resp);
                }

                var session = await _context.Session
                    .AsNoTracking()
                    .Include(x => x.UserNameNavigation)
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync(x => x.Uuid == request.KeyQR);

                if (session == null)
                {
                    resp.error.SetErrorCode(ErrorCode.QR_UNAUTHOR);
                    return new OkObjectResult(resp);
                }

                resp.Data = new LogInResp()
                {
                    Uuid = session.UserNameNavigation.Uuid,
                    Avatar = session.UserNameNavigation.Avatar,
                    FullName = session.UserNameNavigation.FullName ?? session.UserNameNavigation.UserName,
                    UserName = session.UserNameNavigation.UserName,
                    Email = session.UserNameNavigation.Email,
                    RoleId = session.UserNameNavigation.RoleId,

                    SessionUuid = session.Uuid,
                    Token = session.AccessToken,
                    RefreshToken = session.RefreshToken,
                    TimeExpired = session.TimeExpired,
                    TimeExpiredRefresh = session.TimeExpiredRefresh,
                    TimeStart = session.LoginTime,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                resp.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return new OkObjectResult(resp);
        }

        /// <summary>
        /// Quét QR login
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("scan")]
        [Authorize]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponse), description: "Response")]
        public async Task<IActionResult> ScanQRCode([FromBody] QRScanRequest request)
        {
            var accUser = User.GetUserName();
            var accUuid = User.GetAccountUuid();
            var accRoleId = User.GetRoleId();

            var resp = new BaseResponse();

            try
            {
                var qrLogin = await _context.LoginQrCode.AsNoTracking().FirstOrDefaultAsync(x => x.Uuid == request.KeyQR);

                if (qrLogin == null)
                {
                    resp.error.SetErrorCode(ErrorCode.NOT_FOUND);
                    return new OkObjectResult(resp);
                }

                if (qrLogin.TimeExpired < DateTime.UtcNow)
                {
                    resp.error.SetErrorCode(ErrorCode.QR_EXPIRED);
                    return new OkObjectResult(resp);
                }

                var session = _context.Session.AsNoTracking().OrderByDescending(x => x.Id).FirstOrDefault(x => x.Uuid == request.KeyQR);

                if (session != null)
                {
                    resp.error.SetErrorCode(ErrorCode.QR_EIXST);
                    return new OkObjectResult(resp);
                }

                var _authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, accUser),
                    new Claim("AccountUuid", accUuid),
                    new Claim(ClaimTypes.Role, accRoleId),
                    new Claim("FullName", User.GetFullName()),
                    new Claim("Avatar", User.GetAvatar()),
                };

                sbyte? roleId = (sbyte?)(accRoleId == "0" ? 0 : accRoleId == "1" ? 1 : 2);
                var _token = _jwtAuthenticationManager.Authenticate(_authClaims, accUser, accUuid, roleId, keyQR: request.KeyQR);

                if (_token is null)
                {
                    resp.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
                    return new OkObjectResult(resp);
                }

                if (string.IsNullOrEmpty(qrLogin.FcmToken) || qrLogin.FcmToken.Length < 20)
                    qrLogin.FcmToken = null;

                await _context.Session.AddAsync(new Session()
                {
                    Uuid = _token.SessionUuid,
                    DeviceId = qrLogin.DeviceId,
                    UserName = accUser,
                    LoginTime = DateTime.Now,
                    Ip = qrLogin.Ip,
                    FcmToken = qrLogin.FcmToken,
                    Status = 0,
                    AccessToken = _token.AccessToken,
                    RefreshToken = _token.RefreshToken,
                    TimeExpired = (DateTime)_token.TimeExpired,
                    TimeExpiredRefresh = (DateTime)_token.TimeExpiredRefresh
                });

                var newDevice = await _context.Devices.FirstOrDefaultAsync(x => x.UserName == accUser && x.DeviceId == qrLogin.DeviceId);

                if (newDevice is null)
                {
                    await _context.Devices.AddAsync(new Devices()
                    {
                        Os = qrLogin.Os,
                        DeviceId = qrLogin.DeviceId,
                        DeviceName = qrLogin.DeviceName,
                        UserName = accUser,
                        LastUsed = DateTime.Now,
                        Ip = qrLogin.Ip,
                        Address = qrLogin.Address,
                    });
                }
                else
                {
                    newDevice.Ip = qrLogin.Ip;
                    newDevice.Address = qrLogin.Address;
                    newDevice.LastUsed = DateTime.Now;
                    newDevice.Status = 1;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                resp.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
            }

            return new OkObjectResult(resp);
        }
    }
}
