using ChatApp.Models.Request;

namespace ChatApp.Models.Request
{
    public class PinGroupRequest : DpsParamBase
    {
        public String RoomUuid { get; set; }
        public int State { get; set; }
    }
}
