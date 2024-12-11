using ChatApp.Models.DataInfo;

namespace ChatApp.Models.Response
{
    public class FindMemberResponseMessage<T> : BaseResponseMessageItem<T>
    {
        public int MakeFriendState { get; set; }
        public int IsGroupAdmin { get; set; } //0,1
        public MemberDetailDTO? Leader { get; set; } //0,1
        public int TotalCount { get; set; } = 0;
        public int TotalBan { get; set; } = 0;
        public int TotalBlock { get; set; } = 0;
        //public List<T> ItemsLock { get; set; } = new List<T>();
    }
}
