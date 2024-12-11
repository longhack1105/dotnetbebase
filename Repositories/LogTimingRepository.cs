using TWChatAppApiMaster.Databases.ChatApp;

namespace TWChatAppApiMaster.Repositories
{
    public class LogTimingRepository : ILogTimingRepository
    {
        private readonly DBContext _dbContext;

        public LogTimingRepository(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(LogTiming input)
        {
            await _dbContext.AddAsync(input);
            await _dbContext.SaveChangesAsync();
        }
    }
}
