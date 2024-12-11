namespace ChatApp.Models.DataInfo
{
    public class AccountDTO
    {
        public string Uuid { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string PassWord { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? FullName { get; set; }
        public string? Avatar { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime LastUpdated { get; set; }
        /// <summary>
        /// 0: Pending - 1: Active - 2:Deactive
        /// </summary>
        public sbyte Status { get; set; }
        public sbyte ActiveState { get; set; }
        public int Id { get; set; }
        /// <summary>
        /// 0: normal - 1: leader
        /// </summary>
        public sbyte? RoleId { get; set; }
        /// <summary>
        /// 0: not register 1: receive
        /// </summary>
        public sbyte ReceiveNotifyStatus { get; set; }
    }
}
