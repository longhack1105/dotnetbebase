namespace ChatApp.Models.Request
{
    public class UuidAndKeywordAndIsLockRequest : KeywordRequest
    {
        public string Uuid { get; set; }
        public int? IsBan { get; set; }
        public int? IsBlock { get; set; }
    }
}
