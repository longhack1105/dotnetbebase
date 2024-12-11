using TWChatAppApiMaster.Models.DataInfo;

namespace TWChatAppApiMaster.Models.Response.Admin
{
    public class GroupGetListResp
    {
        public long Id { get; set; }
        public string Uuid { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public UserNameBaseDTO Leader { get; set; } = new();
        public int MemberCount {  get; set; }
        public List<Member> Members { get; set; } = new List<Member>();
        public class Member : UserNameBaseDTO
        {
            public string? Avatar { get; set; } 
            public bool IsOnline {  get; set; } = false;
        }

        public int MessageCount { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
