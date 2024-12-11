using System;
using System.Collections.Generic;

namespace ChatApp.Models.DataInfo
{
    public class MsgDeleteDTO
    {
        public int Id { get; set; }
        public string MessageLineUuid { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public DateTime? TimeCreate { get; set; }
        public string RoomUuid { get; set; } = null!;
    }
}
