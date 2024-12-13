namespace DotnetBeBase.Models.Dtos
{
    public class UploadDTO
    {
        public string FilePath {  get; set; } // Đường dẫn tới file
        public string FileExtension {  get; set; } // Đuôi file
        public string? ThumbnailPath { get; set; } // Đường dẫn tới ảnh 1s đầu của video
        public int? VideoDuration {  get; set; } // Số giây chạy hết 1 video
        public float Size {  get; set; } // Dung lượng file
    }
}
