using TWChatAppApiMaster.Databases.ChatApp;

namespace ChatApp.Queue
{
    public class MessageQueueManager
    {
        private static readonly List<MessageChatQueue> QUEUE_MANAGER = new List<MessageChatQueue>();
        private static SemaphoreSlim mutex = new SemaphoreSlim(1, 1);

        public static MessageChatQueue? dequeue()
        {
            mutex.Wait();

            try
            {
                if (QUEUE_MANAGER.Count > 0)
                {
                    var lastItem = QUEUE_MANAGER[0];

                    QUEUE_MANAGER.Remove(lastItem);

                    return lastItem;
                }
            }
            finally
            {
                mutex.Release();
            }

            return null;
        }

        public static void enqueue(MessageChatQueue message)
        {
            mutex.Wait();

            try
            {
                QUEUE_MANAGER.Add(message);
            }
            finally
            {
                mutex.Release();
            }
        }
    }
}
