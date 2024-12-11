
using System.Text.Json.Serialization;

namespace ChatApp.Models.Response
{
    public class BalanceResp : BaseResponse
    {
        public double OriginBalance { get; set; }
    }
}
