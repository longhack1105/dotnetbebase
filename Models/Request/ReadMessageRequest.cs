using System.ComponentModel.DataAnnotations;

namespace ChatApp.Models.Request
{
    public class ReadMessageRequest : DpsParamBase
    {
        public string RoomUuid { get; set; }
        public string MessageLineUuid { get; set; }
    }
}
