using ChatApp.Models.Response;
using System;
using System.Collections.Generic;

namespace ChatApp.Models.DataInfo
{
    public class ResMessagesGroupDTO : BaseResponseMessageItem<MessagesGroupDTO>
    {
        public int UnreadTotal { get; set; }
    }
}
