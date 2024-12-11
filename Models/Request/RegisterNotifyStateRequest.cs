using ChatApp.Models.Request;

namespace ChatApp.Models.Request
{
    public class RegisterNotifyStateRequest : DpsParamBase
    {
        public sbyte? state { get; set; }
    }
}
