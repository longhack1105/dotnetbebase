
using System.Text.Json.Serialization;

namespace ChatApp.Models.Response
{
    public class LogInResp
    {
        public string SessionUuid { get; set; }
        public string Token { get; set; }
        public string? TokenFcm { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public string Uuid { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? FullName { get; set; }
        public int? RoleId { get; set; }
        public string? Avatar { get; set; }
        public DateTime TimeExpired { get; set; }
        public DateTime TimeExpiredRefresh { get; set; }
        public DateTime TimeStart { get; set; }
    }
}
