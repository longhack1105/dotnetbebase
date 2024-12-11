namespace ChatApp.Models.Request
{
    public class UserBaseRequest : DpsParamBase
    {
        public string UserName { get; set; } = string.Empty;
    }
}
