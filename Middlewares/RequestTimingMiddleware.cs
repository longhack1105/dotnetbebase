using System.Diagnostics;
using TWChatAppApiMaster.Databases.ChatApp;
using TWChatAppApiMaster.Repositories;

namespace TWChatAppApiMaster.Middlewares
{
    public class RequestTimingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<RequestTimingMiddleware> _logger;

        public RequestTimingMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory, ILogger<RequestTimingMiddleware> logger)
        {
            _next = next;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value;
            bool isUpload = path.StartsWith("/api/v1/Upload");
            bool isApi = path.StartsWith("/api");

            if (isUpload || !isApi)
            {
                await _next(context);
            }
            else
            {
                var stopwatch = Stopwatch.StartNew();

                // Capture request body
                var request = await FormatRequest(context.Request);

                // Capture response body
                var originalBodyStream = context.Response.Body;
                using (var responseBody = new MemoryStream())
                {
                    context.Response.Body = responseBody;

                    await _next(context);

                    stopwatch.Stop();
                    var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

                    context.Response.Body.Seek(0, SeekOrigin.Begin);
                    var response = await new StreamReader(context.Response.Body).ReadToEndAsync();
                    context.Response.Body.Seek(0, SeekOrigin.Begin);

                    if (context.Request.Path.ToString().Length > 1)
                    {
                        // Log to the database
                        var log = new LogTiming
                        {
                            Name = context.Request.Path,
                            TimeHandle = (int)elapsedMilliseconds,
                            Request = request,
                            Response = response
                        };

                        try
                        {
                            using (var scope = _serviceScopeFactory.CreateScope())
                            {
                                var logTimingRepo = scope.ServiceProvider.GetRequiredService<ILogTimingRepository>();

                                await logTimingRepo.AddAsync(log);
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogError($"RequestTimingMiddleware Error: {e}");
                        }
                    }

                    await responseBody.CopyToAsync(originalBodyStream);
                }
            }
        }

        private async Task<string> FormatRequest(HttpRequest request)
        {
            request.EnableBuffering();
            var body = await new StreamReader(request.Body).ReadToEndAsync();
            request.Body.Position = 0;
            return $"{request.Scheme} {request.Host} {request.QueryString} {body}";
        }
    }
}
