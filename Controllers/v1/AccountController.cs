using DotnetBeBase.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using DotnetBeBase.Databases.Quanlytrungtam;
using DotnetBeBase.Repositories;
using DotnetBeBase.SecurityManagers;
using Microsoft.VisualStudio.Services.Account;
using DotnetBeBase.Models.Basic;
using DotnetBeBase.Models.Request;
using Account = DotnetBeBase.Databases.Quanlytrungtam.Account;

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
        /// Register account
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("register")]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessage), description: "register account")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var resp = new BaseResponseMessage();
            try
            {
                var email = (request.Email ?? "").Trim();
                var phoneNumber = (request.PhoneNumber ?? "").Trim();

                if (request.RegisterType == 0 && string.IsNullOrEmpty(email))
                {
                    resp.error = new BaseResponseMessage.Error(ErrorCode.EMAIL_INVALID);
                    return new OkObjectResult(resp);
                }
                if (request.RegisterType == 1 && string.IsNullOrEmpty(phoneNumber))
                {
                    resp.error = new BaseResponseMessage.Error(ErrorCode.PHONE_INVALID);
                    return new OkObjectResult(resp);
                }
                if (string.IsNullOrEmpty(request.Password))
                {
                    resp.error = new BaseResponseMessage.Error(ErrorCode.PASS_INVALID);
                    return new OkObjectResult(resp);
                }

                var registerName = request.RegisterType == 0 ? email : phoneNumber;

                var acc = _context.Account
                    .Where(x => x.Email == registerName || x.PhoneNumber == registerName)
                    .FirstOrDefault();

                if (acc != null)
                {
                    resp.error = new BaseResponseMessage.Error(ErrorCode.ACCOUNT_EXISTS);
                    return new OkObjectResult(resp);
                }

                var newAcc = new Account()
                {
                    Uuid = Guid.NewGuid().ToString(),
                    Username = registerName,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    Password = request.Password,
                    Role = request.Role,
                    FullName = request.FullName,
                    RegisterType = request.RegisterType,
                };

                //thêm mới thành viên với vai trò của tài khoản
                switch (newAcc.Role)
                {
                    case 1: //giáo viên
                        var newTeacher = new Teacher
                        {
                            Uuid = Guid.NewGuid().ToString(),
                            FullName = newAcc.FullName,
                            Phone = newAcc.PhoneNumber,
                            Email = newAcc.Email,   
                        };
                        await _context.Teacher.AddAsync(newTeacher);
                        break;
                    case 2: //học sinh
                        var newStudent = new Student
                        {
                            Uuid = Guid.NewGuid().ToString(),
                            FullName = newAcc.FullName,
                            Phone = newAcc.PhoneNumber,
                            Email = newAcc.Email,
                        };
                        await _context.Student.AddAsync(newStudent);
                        break;
                }

                //cấp account cho thành viên
                //newAcc.RoleUuid = 

                await _context.Account.AddAsync(newAcc);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error:[{DateTime.Now}] [AccountController][Register] exception : {ex.Message}");
                resp.error = new BaseResponseMessage.Error(ErrorCode.SYSTEM_ERROR);
                return new OkObjectResult(resp);
            }

            return new OkObjectResult(resp);
        }
    }
}
