using System;
using System.Collections.Generic;

namespace TWChatAppApiMaster.Databases.ChatApp
{
    public partial class Account
    {
        public Account()
        {
            Devices = new HashSet<Devices>();
            FriendsUserReceiverNavigation = new HashSet<Friends>();
            FriendsUserSentNavigation = new HashSet<Friends>();
            MessageLike = new HashSet<MessageLike>();
            MessagePin = new HashSet<MessagePin>();
            MessageRead = new HashSet<MessageRead>();
            Messages = new HashSet<Messages>();
            Notifications = new HashSet<Notifications>();
            RegisterAutoDelete = new HashSet<RegisterAutoDelete>();
            RoomBan = new HashSet<RoomBan>();
            RoomBlock = new HashSet<RoomBlock>();
            RoomDelete = new HashSet<RoomDelete>();
            RoomMembers = new HashSet<RoomMembers>();
            RoomPin = new HashSet<RoomPin>();
            RoomRecent = new HashSet<RoomRecent>();
            Rooms = new HashSet<Rooms>();
            Session = new HashSet<Session>();
        }

        public int Id { get; set; }
        public string Uuid { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string PassWord { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? FullName { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime LastUpdated { get; set; }
        /// <summary>
        /// 0: Pending - 1: Active - 2:Deactive
        /// </summary>
        public sbyte Status { get; set; }
        /// <summary>
        /// 0: normal - 1: leader - 2: admin
        /// </summary>
        public sbyte? RoleId { get; set; }
        /// <summary>
        /// 0: not register 1: receive
        /// </summary>
        public sbyte ReceiveNotifyStatus { get; set; }
        /// <summary>
        /// 0: lock 1: active
        /// </summary>
        public sbyte ActiveState { get; set; }
        public string? Avatar { get; set; }
        public ulong IsEnable { get; set; }
        public DateTime LastSeen { get; set; }

        public virtual ICollection<Devices> Devices { get; set; }
        public virtual ICollection<Friends> FriendsUserReceiverNavigation { get; set; }
        public virtual ICollection<Friends> FriendsUserSentNavigation { get; set; }
        public virtual ICollection<MessageLike> MessageLike { get; set; }
        public virtual ICollection<MessagePin> MessagePin { get; set; }
        public virtual ICollection<MessageRead> MessageRead { get; set; }
        public virtual ICollection<Messages> Messages { get; set; }
        public virtual ICollection<Notifications> Notifications { get; set; }
        public virtual ICollection<RegisterAutoDelete> RegisterAutoDelete { get; set; }
        public virtual ICollection<RoomBan> RoomBan { get; set; }
        public virtual ICollection<RoomBlock> RoomBlock { get; set; }
        public virtual ICollection<RoomDelete> RoomDelete { get; set; }
        public virtual ICollection<RoomMembers> RoomMembers { get; set; }
        public virtual ICollection<RoomPin> RoomPin { get; set; }
        public virtual ICollection<RoomRecent> RoomRecent { get; set; }
        public virtual ICollection<Rooms> Rooms { get; set; }
        public virtual ICollection<Session> Session { get; set; }
    }
}
