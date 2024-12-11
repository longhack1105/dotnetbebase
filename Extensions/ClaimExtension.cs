using System.Security.Claims;

namespace ApiBuyerMorgan.Extensions
{
    public static class ClaimExtension
    {
        public static string? GetUserName(this ClaimsPrincipal user) => user.FindFirst(ClaimTypes.Name)?.Value;
        public static string? GetRoleId(this ClaimsPrincipal user) => user.FindFirst(ClaimTypes.Role)?.Value;
        public static string? GetAccountUuid(this ClaimsPrincipal user) => user.FindFirst("AccountUuid")?.Value;
        public static string? GetFullName(this ClaimsPrincipal user) => user.FindFirst("FullName")?.Value;
        public static string? GetAvatar(this ClaimsPrincipal user) => user.FindFirst("Avatar")?.Value;
    }
}
