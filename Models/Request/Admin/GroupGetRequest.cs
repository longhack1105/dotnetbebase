using ChatApp.Models.Request;

namespace TWChatAppApiMaster.Models.Request.Admin
{
    public class GroupGetRequest : KeywordRequest
    {
        public string? LeaderUserName {  get; set; }
    }
}
