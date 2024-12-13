using Microsoft.EntityFrameworkCore;
using DotnetBeBase.Databases.Quanlytrungtam;
using DotnetBeBase.Repositories;

namespace ApiBuyerMorgan.Repositories
{
    public class SessionRepository : ISessionRepository
    {
        private readonly DBContext _context;

        public SessionRepository(DBContext context)
        {
            _context = context;
        }

        public async Task<Session?> GetSession(string token, bool? isUpload = null, bool? admin = null)
        {
            var session = await _context.Session
                .Include(x => x.UsernameNavigation)
                .Where(x => x.TimeExpired > DateTime.UtcNow)
                .Where(x => x.State == 1)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync(x => x.Token == token);

            if (session == null)
            {
                return null;
            } 

            return session;
        }

        public async Task<Session?> GetSessionByRefreshToken(string refreshToken)
        {
            try
            {
                var session = await _context.Session
                .Include(x => x.UsernameNavigation)
                .Where(x => x.State == 1)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);

                if (session == null)
                {
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
