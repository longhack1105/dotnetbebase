using ChatApp.Models.Request;

namespace TWChatAppApiMaster.Models.Request
{
    public class QRScanRequest : DpsParamBase
    {
        public string KeyQR { get; set; }
    }
}
