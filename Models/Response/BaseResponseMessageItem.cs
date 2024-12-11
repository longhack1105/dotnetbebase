namespace ChatApp.Models.Response
{
    public class BaseResponseMessageItem<DTO> : BaseResponse
    {
        public List<DTO> Items { get; set; } = new List<DTO>();
    }
}
