namespace ChatApp.Models.Request
{
    public class UuidRequest : DpsParamBase
    {
        public string Uuid { get; set; }
    }

    public class UuidListRequest : DpsParamBase
    { public string[] UuidList { get; set; } }
}
