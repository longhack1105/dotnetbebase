namespace TWChatAppApiMaster.Models.Response.Admin
{
    public class AccountGetListResp
    {
        public long Id { get; set; }
        public string Uuid { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public sbyte RoleId { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsOnline { get; set; } = false;
        public sbyte ActiveState { get; set; }
    }
}
