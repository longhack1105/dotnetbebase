

using DotnetBeBase.Databases.Quanlytrungtam;

namespace DotnetBeBase.Repositories
{
    public interface ISessionRepository
    {
        Task<Session?> GetSession(string token, bool? isUpload = null, bool? admin = null);
        Task<Session?> GetSessionByRefreshToken(string refreshToken);
    }
}
