namespace ChatApp.Models.Response
{
    public class BaseResponseMessage<T> : BaseResponse
    {
        public T? Data { get; set; }
    }
}
