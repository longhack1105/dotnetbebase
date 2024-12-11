namespace ChatApp.Models.Response
{
    public class MessageLineResponseMessage<T> : BaseResponseMessageItem<T>
    {
        public string? LastMsgReadByMe { get; set; }
        public string? LastMsgRead { get; set; }
        public bool IsBan { get; set; } = false;
    }
}
