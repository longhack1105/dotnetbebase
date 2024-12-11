using ChatApp.Models.Request;

namespace ChatApp.Models.Request
{
    public class GetListFileInGroupRequest : DpsPagingParamBase
    {
        public sbyte? Type { get; set; }
        public String? Uuid { get; set; }
    }
}
