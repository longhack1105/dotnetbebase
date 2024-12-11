using ChatApp.Models.Response;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using TWChatAppApiMaster.Models.DataInfo;

namespace TWChatAppApiMaster.Utils
{

    public class MailService
    {
        public static string _URL = String.Empty;
        private readonly static string projectName = "CHATAPP";
        public static void Init(string URL)
        {
            _URL = URL;
        }
        public static async Task<HttpResponseMessage?> SendMailAsync(string ToEmail, string Title, string Content)
        {
            if (string.IsNullOrEmpty(_URL))
                return null;

            var httpClient = new HttpClient();
            var body = new
            {
                ToEmail,
                Title,
                Content,
                projectName,
            };
            var httpRequestMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{_URL}/api/v1/Mail/SendMail"),
                Headers = {
                                    { HttpRequestHeader.Accept.ToString(), "application/json" },
                                    { HttpRequestHeader.ContentType.ToString(), "application/json" },
                                },
                Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
            };

            var httpResponseMessage = httpClient.Send(httpRequestMessage);
            var result = await httpResponseMessage.Content.ReadAsStringAsync();

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Http Response Code: {httpResponseMessage.StatusCode}");
            }

            return httpResponseMessage;
        }
        public static async Task SendMailWithTemplateAsync(string ToEmail, string Title, string TemplateName, List<string>? KeyReplace, List<string>? ValueReplace)
        {
            var httpClient = new HttpClient();
            var body = new
            {
                toEmail = ToEmail,
                title = Title,
                templateName = TemplateName,
                keyReplace = KeyReplace,
                valueReplace = ValueReplace,
                projectName,
            };

            var bodyJson = JsonConvert.SerializeObject(body);

            var httpRequestMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{_URL}/api/v1/Mail/SendMailWithTemplate"),
                Headers = {
                    { HttpRequestHeader.Accept.ToString(), "application/json" },
                    { HttpRequestHeader.ContentType.ToString(), "application/json" },
                },
                Content = new StringContent(bodyJson, Encoding.UTF8, "application/json")
            };

            var httpResponseMessage = httpClient.Send(httpRequestMessage);
            var result = await httpResponseMessage.Content.ReadAsStringAsync();

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Http Response Code: {httpResponseMessage.StatusCode}");
            }
        }
        public static async Task<BaseResponseMessage<OtpModel>?> GenerateOtpAsync(string email)
        {
            var httpClient = new HttpClient();

            var body = new
            {
                projectName,
                owner = email,
                action = 2,
            };

            var bodyJson = JsonConvert.SerializeObject(body);

            var httpRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{_URL}/api/v1/Mail/generate-otp"),
                Headers = {
                    { HttpRequestHeader.Accept.ToString(), "application/json" },
                    { HttpRequestHeader.ContentType.ToString(), "application/json" },
                },
                Content = new StringContent(bodyJson, Encoding.UTF8, "application/json")
            };

            var resp = httpClient.SendAsync(httpRequest).Result;
            var result = await resp.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<BaseResponseMessage<OtpModel>>(result);
        }
        public static async Task<BaseResponse?> VerifyAsync(string email, string otp)
        {
            var httpClient = new HttpClient();

            var body = new
            {
                projectName,
                owner = email,
                action = 2,
                otp,
            };

            var bodyJson = JsonConvert.SerializeObject(body);

            var httpRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{_URL}/api/v1/Mail/verify"),
                Headers = {
                    { HttpRequestHeader.Accept.ToString(), "application/json" },
                    { HttpRequestHeader.ContentType.ToString(), "application/json" },
                },
                Content = new StringContent(bodyJson, Encoding.UTF8, "application/json")
            };

            var resp = httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead).Result;
            var result = await resp.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<BaseResponse>(result);
        }

        /// <summary>
        /// Tạo Otp + Gửi sms cho số diện thoại
        /// </summary>
        /// <param name="Phone"></param>
        /// <param name="Action"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<BaseResponse?> SendSmsAsync(string Phone, int Action)
        {
            if (string.IsNullOrEmpty(_URL))
                return null;

            var httpClient = new HttpClient();
            var body = new
            {
                Phone,
                Action,
                projectName,
            };
            var httpRequestMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{_URL}/api/v1/Sms/send"),
                Headers = {
                    { HttpRequestHeader.Accept.ToString(), "application/json" },
                    { HttpRequestHeader.ContentType.ToString(), "application/json" },
                },
                Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
            };

            var httpResponseMessage = httpClient.Send(httpRequestMessage);
            var result = await httpResponseMessage.Content.ReadAsStringAsync();

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Http Response Code: {httpResponseMessage.StatusCode}");
            }

            return JsonConvert.DeserializeObject<BaseResponse>(result);
        }

        /// <summary>
        /// Kiểm tra otp 
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="otp"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<BaseResponse?> VerifySmsAsync(string phone, string otp, int action)
        {
            if (string.IsNullOrEmpty(_URL))
                return null;

            var httpClient = new HttpClient();
            var body = new
            {
                owner = phone,
                action,
                otp,
                projectName,
            };
            var httpRequestMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{_URL}/api/v1/Sms/verify"),
                Headers = {
                    { HttpRequestHeader.Accept.ToString(), "application/json" },
                    { HttpRequestHeader.ContentType.ToString(), "application/json" },
                },
                Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
            };

            var httpResponseMessage = httpClient.Send(httpRequestMessage);
            var result = await httpResponseMessage.Content.ReadAsStringAsync();

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Http Response Code: {httpResponseMessage.StatusCode}");
            }

            return JsonConvert.DeserializeObject<BaseResponse>(result);
        }
    }
}