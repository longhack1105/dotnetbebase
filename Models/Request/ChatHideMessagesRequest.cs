using ChatApp.Models.Request;

namespace TWChatAppApiMaster.Models.Request
{
    public class ChatHideMessagesRequest : UuidListRequest
    {
        public string RoomUuid {  get; set; } = string.Empty;   
        public string? UserName {  get; set; } 
    }
}
