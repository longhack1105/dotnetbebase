using ChatApp.Configuaration;

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DotnetBeBase.Models;

namespace DotnetBeBase.SecurityManagers
{
    public class JwtAuthenticationManager : IJwtAuthenticationManager
    {
        /// <summary>
        /// Hàm thực hiện cho việc login lấy token
        /// </summary>
        /// <param name="_authClaims"></param>
        /// <param name="_username">Tên đăng nhập người dùng</param>
        /// <param name="_uuid">Uuid của tài khoản</param>
        /// <param name="_roleId">Kiểu tài khoản</param>
        /// <param name="keyQR">key đăng nhập bằng QR code</param>
        /// <returns></returns>
        public TokenModel? Authenticate(List<Claim> _authClaims, string _username, string _uuid, sbyte? _roleId, string? keyQR = null)
        {
            if (string.IsNullOrEmpty(_username))
                return null;
            if (string.IsNullOrEmpty(_uuid))
                return null;
            if (_roleId is null || _roleId < 0 || _roleId > 2)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();

            // khởi tạo token
            var tokenDescriptor = CreateToken(_authClaims, tokenHandler);
            // khởi tạo mã refresh token
            var _refreshToken = GenerateRefreshToken();
            DateTime _timeExpiredRefresh = DateTime.Now.AddDays(value: GlobalSettings.AppSettings.TokenSettings.RefreshTokenExpirationTime);
            return new TokenModel()
            {
                SessionUuid = !string.IsNullOrEmpty(keyQR) ? keyQR : Guid.NewGuid().ToString(),
                AccessToken = tokenHandler.WriteToken(tokenDescriptor),
                TimeExpired = tokenDescriptor.ValidTo.ToLocalTime(),
                RefreshToken = _refreshToken,
                TimeStart = tokenDescriptor.IssuedAt.ToLocalTime(),
                TimeExpiredRefresh = _timeExpiredRefresh
            };
        }
        /// <summary>
        /// Hàm tạo ra token with giá trị cần điền vô claim
        /// </summary>
        /// <param name="authClaims">Danh sach claim</param>
        /// <returns></returns>
        private JwtSecurityToken CreateToken(List<Claim> authClaims, JwtSecurityTokenHandler _tokenHandler)
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var tokenKey = Encoding.UTF8.GetBytes(GlobalSettings.AppSettings.TokenSettings.Secret);

            var sercurityKey = new SymmetricSecurityKey(tokenKey);

            var credentials = new SigningCredentials(sercurityKey, SecurityAlgorithms.HmacSha256Signature);

            var token = _tokenHandler.CreateJwtSecurityToken(
                subject: new ClaimsIdentity(authClaims),
                notBefore: DateTime.Now,
                expires: DateTime.Now.AddMinutes(GlobalSettings.AppSettings.TokenSettings.TokenValidityTime),
                issuedAt: DateTime.Now,
                signingCredentials: credentials
                );
            return token;
        }
        /// <summary>
        /// Hàm tạo ra Key refresh
        /// </summary>
        /// <returns></returns>
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        /// <summary>
        /// Xử lý lấy thông tin từ claim từ token, check tocken
        /// </summary>
        /// <param name="_token"></param>
        /// <returns></returns>
        /// <exception cref="SecurityTokenException"></exception>
        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string? _token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(s: GlobalSettings.AppSettings.TokenSettings.Secret)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(_token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;

        }

        public async Task<TokenResult> RefreshTokenAsync(string accessToken)
        {

            var principal = GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
            {
                return new TokenResult { IsSuccess = false };
            }

            var userName = principal.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return new TokenResult { IsSuccess = false };
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var newAccessToken = CreateToken(principal.Claims.ToList(), tokenHandler);

            var _refreshToken = GenerateRefreshToken();
            DateTime _timeExpiredRefresh = DateTime.Now.AddDays(value: GlobalSettings.AppSettings.TokenSettings.RefreshTokenExpirationTime);
            return new TokenResult()
            {
                IsSuccess = true,
                AccessToken = tokenHandler.WriteToken(newAccessToken),
                TimeExpired = newAccessToken.ValidTo.ToLocalTime(),
                TimeStart = newAccessToken.IssuedAt.ToLocalTime(),
                RefreshToken = _refreshToken,
                TimeExpiredRefresh = _timeExpiredRefresh
            };
        }
    }
}
