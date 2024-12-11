namespace ChatApp.Models.Request
{
    public class RegisterRequest : LogInRequest
    {
        public string FullName { get; set; }
        public int RoleId { get; set; }

        public string OtpCode { get; set; }
        public string Email { get; set; }
    }
}
