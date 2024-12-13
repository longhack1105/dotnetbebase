using System.ComponentModel.DataAnnotations;
using DotnetBeBase.Models.Request;
using DotnetBeBase.Models.Basic;

namespace DotnetBeBase.Models.Request
{
    public class UploadImageRequest : DpsParamBase
    {
        [Required]
        public List<IFormFile> ImageFile { get; set; }
        public int Type { get; set; }
    }
}
