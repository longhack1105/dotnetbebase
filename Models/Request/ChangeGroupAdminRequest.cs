using ChatApp.Models.Request;

namespace ChatApp.Models.Request
{
    public class ChangeGroupAdminRequest : DpsParamBase
    {
        public String RoomUuid { get; set; }
        public String? UserName { get; set; }
    }
}
