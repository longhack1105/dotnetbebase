using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using DotnetBeBase.Extensions;
using DotnetBeBase.Repositories;

namespace DotnetBeBase.Middleware
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public SessionValidationMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
        {
            _next = next;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            var actionDescriptor = endpoint?.Metadata?.GetMetadata<ControllerActionDescriptor>();

            // Kiểm tra xem action hoặc controller có gắn [Authorize] không
            bool hasAuthorize = actionDescriptor?.MethodInfo.GetCustomAttributes(typeof(AuthorizeAttribute), true).Any() == true ||
                                actionDescriptor?.ControllerTypeInfo.GetCustomAttributes(typeof(AuthorizeAttribute), true).Any() == true;

            if (hasAuthorize)
            {
                var token = context.GetToken();
                var path = context.Request.Path.Value;

                if (!string.IsNullOrEmpty(token))
                {
                    bool isUpload = path.StartsWith("/api/v1/Upload");
                    bool isAdmin = path.StartsWith("/api/v2");

                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var sessionRepository = scope.ServiceProvider.GetRequiredService<ISessionRepository>();
                        var session = await sessionRepository.GetSession(token, isUpload ? null : isAdmin);

                        if (session == null)
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.Headers.Add("WWW-Authenticate", "Bearer error=\"invalid_token\"");
                            context.Response.ContentType = "text/plain";
                            return;
                        }
                    }
                }
            }

            // Nếu session hợp lệ hoặc không cần kiểm tra, chuyển tiếp đến middleware tiếp theo
            await _next(context);
        }
    }
}
