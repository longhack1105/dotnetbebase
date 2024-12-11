using TWChatAppApiMaster.Databases.ChatApp;

namespace TWChatAppApiMaster.Repositories
{
    public interface ISessionRepository
    {
        Task<Session?> GetSession(string token, bool? isAdmin = null);
        Task<Session?> GetSessionByRefreshToken(string refreshToken);
    }
}
