using System.ComponentModel.DataAnnotations;

namespace ChatApp.Models.Request
{
    public class ChangePasswordRequest : UserBaseRequest
    {
        [Required]
        public string OldPass { get; set; }
        [Required]
        public string NewPass { get; set; }
    }
}
