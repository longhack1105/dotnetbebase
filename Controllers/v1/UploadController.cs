using ApiBuyerMorgan.Extensions;
using ChatApp.Configuaration;
using ChatApp.Enum;
using ChatApp.Models.Request;
using ChatApp.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TWChatAppApiMaster.Databases.ChatApp;
using TWChatAppApiMaster.Models.DataInfo;
using TWChatAppApiMaster.Models.Request;

namespace ChatApp.Controllers.v1
{
    [Authorize]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class UploadController : ControllerBase
    {
        private readonly DBContext _context;
        private readonly ILogger<UploadController> _logger;
        public UploadController(DBContext context, ILogger<UploadController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Upload ảnh đại diện - Tối đa 20Mb
        /// </summary>
        /// <returns></returns>
        [HttpPut("upload-image")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(20 * 1024 * 1024)]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessageItem<string>), description: "UploadImage Response")]
        public async Task<IActionResult> UploadImage([FromForm] UploadImageRequest request)
        {
            var accUser = User.GetUserName();

            var response = new BaseResponseMessageItem<string>();

            try
            {
                if(request.ImageFile.Count() > 0)
                {
                    for(int i = 0; i < request.ImageFile.Count(); i++)
                    {
                        var ext = Path.GetExtension(request.ImageFile[i].FileName);
                        var newUuid = Guid.NewGuid().ToString();

                        var filename = $"{newUuid}{ext}";
                        string PathResource = Environment.CurrentDirectory;
                        var filePath = Path.Combine(GlobalSettings.AppSettings.UploadPath, filename);
                        string PathFile = Path.Combine(PathResource, filePath);
                        using (var stream = System.IO.File.Create(PathFile))
                        {
                            await request.ImageFile[i].CopyToAsync(stream);
                        }

                        if (request.Type.ToString().Contains("4"))
                        {
                            var file = new FilesInfo()
                            {
                                Uuid = newUuid,
                                UserUpload = accUser,
                                FileName = request.ImageFile[i].FileName,
                                TimeCreated = DateTime.Now,
                                Path = filePath
                            };
                            _context.FilesInfo.Add(file);
                            _context.SaveChanges();
                        }
                        
                        response.Items.Add(filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);

                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
                response.error.Message = ex.Message;
            }

            return Ok(response);
        }

        /// <summary>
        /// Upload file dùng cho chat - Tối đa 20MB
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("upload-file")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(20 * 1024 * 1024)]
        [SwaggerResponse(statusCode: 200, type: typeof(BaseResponseMessage<UploadDTO>), description: "UploadImage Response")]
        public async Task<IActionResult> UploadFile([FromForm] UploadFileRequest request)
        {
            var accUser = User.GetUserName();
            var response = new BaseResponseMessage<UploadDTO>();
            string PathResource = Environment.CurrentDirectory;

            try
            {
                var ext = Path.GetExtension(request.File.FileName);
                var newUuid = Guid.NewGuid().ToString();

                string? thumbFilePath = null;
                if (IsVideoFile(ext))
                {
                    if (request.ThumbnailFile != null)
                    {
                        var thumbExt = Path.GetExtension(request.ThumbnailFile.FileName);
                        var thumbFilename = $"{newUuid}-thumbnail{thumbExt}";
                        thumbFilePath = Path.Combine(GlobalSettings.AppSettings.UploadPath, thumbFilename);
                        string thumbPathFile = Path.Combine(PathResource, thumbFilePath);

                        // Lưu ảnh thumb
                        using (var stream = System.IO.File.Create(thumbPathFile))
                        {
                            await request.ThumbnailFile.CopyToAsync(stream);
                        }
                    }
                }

                var filename = $"{newUuid}{ext}";
                var filePath = Path.Combine(GlobalSettings.AppSettings.UploadPath, filename);
                string PathFile = Path.Combine(PathResource, filePath);

                // Lưu File
                using (var stream = System.IO.File.Create(PathFile))
                {
                    await request.File.CopyToAsync(stream);
                }

                var uploadDTO = new UploadDTO
                {
                    FilePath = filePath,
                    FileExtension = ext,
                    ThumbnailPath = thumbFilePath,
                    VideoDuration = request.VideoDuration,
                    Size = new FileInfo(PathFile).Length,
                };

                var file = new FilesInfo()
                {
                    Uuid = newUuid,
                    UserUpload = accUser,
                    FileName = request.File.FileName,
                    TimeCreated = DateTime.Now,
                    Path = filePath
                };
                _context.FilesInfo.Add(file);
                _context.SaveChanges();

                response.Data = uploadDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);

                response.error.SetErrorCode(ErrorCode.SYSTEM_ERROR);
                response.error.Message = ex.Message;
            }

            return Ok(response);
        }

        private bool IsVideoFile(string extension)
        {
            var videoExtensions = new List<string> { ".mp4", ".avi", ".mov", ".mkv", ".wmv", ".flv" };
            return videoExtensions.Contains(extension.ToLower());
        }
    }
}