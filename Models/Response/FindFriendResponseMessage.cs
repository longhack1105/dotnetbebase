namespace ChatApp.Models.Response
{
    public class FindFriendResponseMessage<T> : BaseResponseMessageItem<T>
    {
        public int Count { get; set; }
    }
}
