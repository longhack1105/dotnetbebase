namespace ChatApp.Models.Request
{
    public class FriendRequestReq : DpsPagingParamBase
    {
        public string? Keyword { get; set; }
        public bool isSend { get; set; }
    }
}
