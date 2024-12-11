namespace TWChatAppApiMaster.Models.Request
{
    public class ForgotEmailStage1Request
    {
        public string Email {  get; set; }
    }

    public class ForgotEmailStage2Request : ForgotEmailStage1Request
    {
        public string Code { get; set; }
    }

    public class ForgotEmailStage3Request : ForgotEmailStage2Request
    {
        public string NewPassword { get; set; }
    }
}
