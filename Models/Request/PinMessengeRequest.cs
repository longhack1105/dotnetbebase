using ChatApp.Models.Request;

namespace ChatApp.Models.Request
{
    public class PinMessengeRequest : DpsParamBase
    {
        public string RoomUuid { get; set; }
        public string MessengeUuid { get; set; }
        public int State { get; set; }
    }
}
