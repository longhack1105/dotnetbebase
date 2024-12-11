using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ChatApp.Models.Request
{
    public class DpsPagingParamBase : DpsParamBase
    {
        [DefaultValue(20)]
        public int PageSize { get; set; } = 20;
        [DefaultValue(1)]
        public int Page { get; set; } = 1;
    }
}
