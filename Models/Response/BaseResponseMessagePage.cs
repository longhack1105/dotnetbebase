namespace ChatApp.Models.Response
{
    public class BaseResponseMessagePage<DTO> : BaseResponse
    {
        public List<DTO> Items { get; set; } = new List<DTO>();
        public Paginations Pagination { get; set; } = new();
        public class Paginations
        {
            public int TotalCount { get; set; } = 0;
            public int TotalPage { get; set; } = 0;
        }
    }
}
