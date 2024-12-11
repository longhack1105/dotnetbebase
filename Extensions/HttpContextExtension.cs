namespace TWChatAppApiMaster.Extensions
{
    public static class HttpContextExtension
    {
        public static string? GetToken(this HttpContext httpContext)
        {
            // Lấy giá trị của Authorization header từ yêu cầu HTTP
            string authorizationHeader = httpContext.Request.Headers["Authorization"];
            if (authorizationHeader != null && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authorizationHeader.Substring("Bearer ".Length).Trim();
            }

            return null;
        }
    }
}
