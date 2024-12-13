using DotnetBeBase.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Configuaration
{
    public class ErrorException : Exception
    {
        public ErrorCode Code { get; set; }
        public string Message { get; set; }

        public ErrorException(ErrorCode code = ErrorCode.SUCCESS, string message = "")
        {
            Code = code;
            Message = message;
        }
    }
}
