using ChatApp.Models.Request;

namespace ChatApp.Models.Request
{
    public class AutoDeleteRequest : DpsParamBase
    {
        public String? RoomUuid { get; set; }
        public int Period { get; set; } //by day
    }
}
