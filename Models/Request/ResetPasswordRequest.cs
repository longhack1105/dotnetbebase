using System.ComponentModel.DataAnnotations;

namespace ChatApp.Models.Request
{
    public class ResetPasswordRequest : UserBaseRequest
    {
        public string OtpCode { get; set; } = string.Empty;
        [Required]
        public string NewPass { get; set; }
    }
}
