namespace DotnetBeBase.SecurityManagers
{
    public class TokenResult
    {
        public bool IsSuccess {  get; set; }
        public string? SessionUuid {  get; set; }
        public string? AccessToken {  get; set; }
        public string? RefreshToken {  get; set; }
        public DateTime TimeExpired {  get; set; }
        public DateTime TimeExpiredRefresh {  get; set; }
        public DateTime TimeStart {  get; set; }
    }
}
