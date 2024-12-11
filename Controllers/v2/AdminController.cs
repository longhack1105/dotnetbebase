using ChatApp.Controllers.v1;
using ChatApp.Enum;
using ChatApp.Models.DataInfo;
using ChatApp.Models.Request;
using ChatApp.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TWChatAppApiMaster.Databases.ChatApp;

namespace ChatApp.Controllers.v2
{
    [Authorize]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("2.0")]
    [SwaggerTag("Admin Controller")]
    public class AdminController : ControllerBase
    {
        private readonly DBContext _context;
        private readonly ILogger<MemberController> _logger;
        public AdminController(DBContext context, ILogger<MemberController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("list-account")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessageItem<AccountDTO>), description: "List account Response")]
        public async Task<IActionResult> ListAccount([FromBody] GetListAccountRequest request)
        {
            var response = new BaseResponseMessageItem<AccountDTO>();

            try
            {
                var lstAccount = _context.Account.Where(x => x.RoleId == request.RoleId)
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
                            Status = item.Status
                        };

                        response.Items.Add(frdDto);
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

        [HttpPost("toggle-lock-account")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponse), description: "List account Response")]
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
    }
}
