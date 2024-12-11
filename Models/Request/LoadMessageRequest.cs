namespace ChatApp.Models.Request
{
    public class LoadMessageRequest : KeywordRequest
    {
        public string MsgGroupUuid { get; set; }
    }
}
