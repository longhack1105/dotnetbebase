using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Utils
{
    public class TokenInfo
    {
        public string Token { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string AccountUuid { get; set; } = string.Empty;
        
        private DateTime ExpiredDate { get; set; }

        public bool IsExpired()
        {
            return ExpiredDate < DateTime.Now;
        }

        public void ResetExpired()
        {
            ExpiredDate = DateTime.Now.AddMonths(1);
        }
    }
}
