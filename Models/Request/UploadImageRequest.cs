using System.ComponentModel.DataAnnotations;
using ChatApp.Models.Request;

namespace ChatApp.Models.Request
{
    public class UploadImageRequest : DpsParamBase
    {
        [Required]
        public List<IFormFile> ImageFile { get; set; }
        public int Type { get; set; }
    }
}
