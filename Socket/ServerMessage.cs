using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Socket
{
    public class ServerMessage
    {
        public ServerMessage()
        {
        }

        public string UserSent { get; set; }
        public sbyte ContentType { get; set; }
        public string Content { get; set; }
        public string MsgGroupUuid { get; set; }
        
        public string MsgLineUuid { get; set; }
        public string CountryCode { get; set; }

    }
}
