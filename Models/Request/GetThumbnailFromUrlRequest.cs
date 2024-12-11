using ChatApp.Models.Request;

namespace ChatApp.Models.Request
{
    public class GetThumbnailFromUrlRequest : DpsParamBase
    {
        public string Url { get; set; }
    }
}
