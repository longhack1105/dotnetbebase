namespace TWChatAppApiMaster.Models.Request.Admin
{
    public class AccountAddRequest : AccountUpdateRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
