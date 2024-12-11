using ChatApp.Enum;
using ChatApp.Models.Response;
using Newtonsoft.Json;

namespace ChatApp.Extensions
{
    public static class SupportExtension
    {
        public static T GetMessage<T>(this T resp, ErrorCode errorCode) where T : BaseResponse
        {

            resp.error = new(errorCode);

            return resp;
        }

        public static string ToJsonString<T>(this T data)
        {
            return JsonConvert.SerializeObject(data);
        }

        public static string? EnumDescription<T>(int idx) where T : System.Enum
        {
            string resp = string.Empty;
            foreach (var value in System.Enum.GetValues(typeof(T)))
            {
                if ((int)value == idx)
                {
                    resp = value.ToDescriptionString();
                    break;
                }
            }

            return resp;
        }
        public static string GetExceptionMessages(Exception e, string msgs = "")
        {
            if (e == null) return string.Empty;
            if (msgs == "") msgs = e.Message;
            if (e.InnerException != null)
                msgs += "\r\nInnerException: " + GetExceptionMessages(e.InnerException);
            return msgs;
        }
    }
}
