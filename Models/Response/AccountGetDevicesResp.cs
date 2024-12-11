namespace TWChatAppApiMaster.Models.Response
{
    public class AccountGetDevicesResp
    {
        public string? DeviceId {  get; set; }
        public string? DeviceName {  get; set; }
        public string? Os {  get; set; }
        public string? Address {  get; set; }
        public string? Ip {  get; set; }
        public DateTime? TimeLastUsed {  get; set; }
    }
}
