
using System.Security.Claims;
using DotnetBeBase.Models;

namespace DotnetBeBase.SecurityManagers
{
    public interface IJwtAuthenticationManager
    {
        /// <summary>
        /// Hàm thực hiện cho việc login lấy token
        /// </summary>
        /// <param name="AuthClaims"></param>
        /// <param name="_username"></param>
        /// <param name="_uuid"></param>
        /// <param name="_roleId"></param>
        /// <param name="keyQR">Key đăng nhập bằng QRCode</param>
        /// <returns></returns>
        TokenModel? Authenticate(List<Claim> AuthClaims,string _username, string _uuid, sbyte? _roleId = null, string? keyQR = null);
        /// <summary>
        /// Xử lý lấy thông tin từ claim từ token, check token
        /// </summary>
        /// <param name="_accessToken"></param>
        /// <returns></returns>
        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string _accessToken);

        Task<TokenResult> RefreshTokenAsync(string refreshToken);
    }
}
