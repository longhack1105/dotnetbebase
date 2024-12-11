using System.ComponentModel.DataAnnotations;
using ChatApp.Models.Request;

namespace ChatApp.Models.Request
{
    public class CreateMessageRoomRequest : DpsParamBase
    {
        public sbyte Type { get; set; }
        public string? OwnerUuid{ get; set; }
    }
}
