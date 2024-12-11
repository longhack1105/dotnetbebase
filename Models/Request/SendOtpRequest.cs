using ChatApp.Models.Request;

namespace ChatApp.Models.Request
{
    public class SendOtpRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int Action { get; set; }
    }
}
