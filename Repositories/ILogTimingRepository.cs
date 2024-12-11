using TWChatAppApiMaster.Databases.ChatApp;

namespace TWChatAppApiMaster.Repositories;

public interface ILogTimingRepository
{
    Task AddAsync(LogTiming input);
}
