using ApiBuyerMorgan.Extensions;
using ChatApp.Configuaration;
using DotnetBeBase.Enums;
using ChatApp.Extensions;
using DotnetBeBase.Models.Request;
using DotnetBeBase.Models.Response;
using ChatApp.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using DotnetBeBase.Databases.Quanlytrungtam;
using DotnetBeBase.Extensions;
using DotnetBeBase.Models.Request;
using DotnetBeBase.Repositories;
using DotnetBeBase.SecurityManagers;

using static Google.Apis.Requests.BatchRequest;

namespace DotnetBeBase.Controllers.v2
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

        
    }
}
