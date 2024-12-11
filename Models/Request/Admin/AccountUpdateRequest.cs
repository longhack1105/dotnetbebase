namespace TWChatAppApiMaster.Models.Request.Admin
{
    public class AccountUpdateRequest
    {
        public string FullName {  get; set; } = string.Empty;
        internal sbyte RoleId { get; set; } = 2;
    }
}
