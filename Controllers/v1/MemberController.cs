using ApiBuyerMorgan.Extensions;
using ChatApp.Enum;
using ChatApp.Models.DataInfo;
using ChatApp.Models.Request;
using ChatApp.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using TWChatAppApiMaster.Databases.ChatApp;

namespace ChatApp.Controllers.v1
{
    [Authorize]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class MemberController : ControllerBase
    {
        private readonly DBContext _context;
        private readonly ILogger<MemberController> _logger;
        public MemberController(DBContext context, ILogger<MemberController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Chi tiết thành viên
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("detail-member")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessage<MemberDetailDTO>), description: "Member Detail Response")]
        public async Task<IActionResult> MemberDetail([FromBody] UuidRequest request)
        {
            var accUser = User.GetUserName();
            var accUuid = User.GetAccountUuid();

            var response = new BaseResponseMessage<MemberDetailDTO>();

            try
            {
                var member = _context.Account.Where(x => x.Uuid == request.Uuid).SingleOrDefault();

                if (member != null)
                {
                    response.Data = new MemberDetailDTO
                    {
                        Uuid = member.Uuid,
                        UserName = member.UserName,
                        FullName = member.FullName,
                        TimeCreated = member.TimeCreated,
                        Status = member.Status,
                        Avatar = member.Avatar,
                    };
                }
                else
                {
                    response.error.SetErrorCode(ErrorCode.INVALID_PARAM);
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
        /// Tìm thành viên
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("find-member")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessageItem<FindMemberDTO>), description: "Find Members Response")]
        public async Task<IActionResult> FindMembers([FromBody] KeywordRequest request)
        {
            var accRoleId = User.GetRoleId();
            var response = new BaseResponseMessageItem<FindMemberDTO>();

            try
            {
                var lstAccounts = await _context.Account
                    .AsNoTracking()
                    .Where(x => x.Status == 1)
                    .Where(x => x.RoleId != 2)
                    .Where(x => EF.Functions.Like(x.FullName ?? "", $"%{request.Keyword}%")
                        || (
                            accRoleId == "0"
                                && (
                                    x.FriendsUserReceiverNavigation.Any(f => f.UserSentNavigation != null && EF.Functions.Like(f.UserSentNavigation.FullName, $"%{request.Keyword}%") && x.Status == 3)
                                    || x.FriendsUserSentNavigation.Any(f => f.UserReceiverNavigation != null && EF.Functions.Like(f.UserReceiverNavigation.FullName, $"%{request.Keyword}%") && x.Status == 3)
                                )
                            )
                    )
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(x => new FindMemberDTO()
                    {
                        Uuid = x.Uuid,
                        UserName = x.UserName,
                        FullName = x.FullName ?? "",
                        Avatar = x.Avatar ?? "",
                        LastSeen = x.LastSeen,
                        RoleId = x.RoleId,
                    })
                    .ToListAsync();

                response.Items = lstAccounts;
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
