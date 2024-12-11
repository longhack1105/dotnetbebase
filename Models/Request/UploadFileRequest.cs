using ChatApp.Models.Request;
using Microsoft.AspNetCore.Mvc;

namespace TWChatAppApiMaster.Models.Request;

public class UploadFileRequest
{
    public IFormFile File { get; set; }

    public IFormFile? ThumbnailFile { get; set; } // Ảnh thumb của video
    public int? VideoDuration { get; set; } // Thời gian chạy của video tính bằng giây
}

public class UploadMutiFileRequest
{
    [FromForm]
    public List<IFormFile> Files { get; set; } = new List<IFormFile>();

    [FromForm]
    public List<IFormFile?> ThumbnailFile { get; set; } = new List<IFormFile?>(); // Ảnh thumb của video
}

