using ChatApp.Enum;
using ChatApp.Extensions;

namespace ChatApp.Models.Response
{
    public class BaseResponse
    {
        public Error error { get; set; } = new Error();
        public class Error
        {
            public ErrorCode Code { get; set; }
            public string Message { get; set; }
            public Error(ErrorCode code = ErrorCode.SUCCESS)
            {
                Code = code;
                Message = code.ToDescriptionString();
            }

            public void SetErrorCode(ErrorCode code)
            {
                Code = code;
                Message = code.ToDescriptionString();
            }
        }
    }
}
