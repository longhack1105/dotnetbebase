using ChatApp.Models.Request;

namespace ChatApp.Models.Request
{
    public class LockAccountRequest : DpsParamBase
    {
        public String Uuid { get; set; }
        public int ActiveState { get; set; }
    }
}
