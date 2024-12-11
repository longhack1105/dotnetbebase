using Microsoft.TeamFoundation.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Socket
{
    public class ClientMessage
    {
        /// <summary>
        /// 1: PP - 2: Group
        /// </summary>
        public sbyte Type { get; set; }
        public string Receiver { get; set; }
        public string Content { get; set; }
        public string CountryCode { get; set; }
        /// <summary>
        /// 1: Text - 2: Link - 3: Image - 4: Video - 5: Audio
        /// </summary>
        public sbyte ContentType { get; set; }
        public string ReplyMsgUuid { get; set; }
        public string FileInfo { get; set; } = string.Empty;

        public bool IsValid()
        {
            if (string.IsNullOrEmpty(this.Receiver) || string.IsNullOrEmpty(this.Content))
            {
                return false;
            }

            return true;
        }
    }
}
