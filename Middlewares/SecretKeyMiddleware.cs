using ChatApp.Configuaration;
using ChatApp.Utils;
using System.Net;

public class SecretKeyMiddleware
{
    private readonly RequestDelegate _next;

    public SecretKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var request = context.Request;
            if (!request.HasFormContentType || !request.Form.ContainsKey("keyCert") || !request.Form.ContainsKey("time"))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Invalid Certificate");
                return;
            }

            var keyCert = request.Form["keyCert"].ToString();
            var time = request.Form["time"].ToString();

            string s = $"{GlobalSettings.AppSettings.DPS_CERT}{time}";
            string expectedKey = MD5Util.Encrypt(s);

            if (keyCert != expectedKey)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Invalid Certificate");
                return;
            }

            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("Invalid Certificate");
        }
    }
}
