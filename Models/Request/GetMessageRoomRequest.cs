namespace ChatApp.Models.Request
{
    public class GetMessageRoomRequest : KeywordRequest
    {
        public int Type { get; set; }  //0: all - 1: find
    }
}
