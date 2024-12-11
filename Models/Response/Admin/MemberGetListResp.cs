namespace TWChatAppApiMaster.Models.Response.Admin
{
    public class MemberGetListResp
    {
        public long Id {  get; set; }
        public string Uuid { get; set; }
        public string FullName {  get; set; } = string.Empty;
        public bool IsOnline { get; set; } = false;
        public int TotalMessage {  get; set; }
        public int TotalGroup { get; set; }
        public sbyte? RoleId { get; set; }
        public List<GroupDTO> Groups { get; set; } = new List<GroupDTO>();
        public class GroupDTO
        {
            public string Uuid { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Avatar { get; set; } = string.Empty;
            public int MemberCount {  get; set; }
        }
        public DateTime TimeCreated { get; set; }
        public DateTime? LastUpdated { get; set; }
        public sbyte ActiveState {  get; set; }
    }
}
