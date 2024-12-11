using Microsoft.EntityFrameworkCore;
using TWChatAppApiMaster.Databases.ChatApp;
using TWChatAppApiMaster.Repositories;

namespace ApiBuyerMorgan.Repositories
{
    public class SessionRepository : ISessionRepository
    {
        private readonly DBContext _context;

        public SessionRepository(DBContext context)
        {
            _context = context;
        }

        public async Task<Session?> GetSession(string token, bool? isAdmin = null)
        {
            var session = await _context.Session
                .Include(x => x.UserNameNavigation)
                .Where(x => x.TimeExpired > DateTime.UtcNow)
                .Where(x => x.Status == 0)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync(x => x.AccessToken == token);

            if (session == null)
            {
                return null;
            }

            if (isAdmin == true && session.UserNameNavigation.RoleId != 2)
            {
                return null;
            }

            if (isAdmin == false && session.UserNameNavigation.RoleId == 2)
            {
                return null;
            }

            if (session.UserNameNavigation.ActiveState == 0)
            {
                session.Status = 1;
                session.LogoutTime = DateTime.Now;
                await _context.SaveChangesAsync();
                return null;
            }    

            return session;
        }

        public async Task<Session?> GetSessionByRefreshToken(string refreshToken)
        {
            try
            {
                var session = await _context.Session
                .Include(x => x.UserNameNavigation)
                .Where(x => x.Status == 0)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);

                if (session == null)
                {
                    return null;
                }

                if (session.TimeExpiredRefresh < DateTime.UtcNow)
                {
                    session.Status = 1;
                    session.LogoutTime = session.TimeExpiredRefresh;
                    _context.Update(session);
                    await _context.SaveChangesAsync();
                    return null;
                }

                if (session.UserNameNavigation.ActiveState == 0)
                {
                    session.Status = 1;
                    session.LogoutTime = DateTime.Now;
                    _context.Update(session);
                    await _context.SaveChangesAsync();
                    return null;
                }

                return session;
            }
            catch (Exception ex)
            {
                return null;
                throw;
            }
            
        }
    }
}
