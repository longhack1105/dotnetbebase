using ChatApp.Models.DataInfo;
using TWChatAppApiMaster.Models.DataInfo;

namespace TWChatAppApiMaster.Socket
{
    public class Message
    {
        public int MsgType { get; set; }
        public string Data { get; set; }
    }

    public class TypingRequest
    {
        public string RoomUuid { get; set; }
        
    }

    public class TypingResponse
    {
        public string FullName { get; set; }
        public string RoomUuid { get; set; }
    }

    public class ReadRequest
    {
        public string RoomUuid { get; set; }
        public string MsgUuid { get; set; }
    }

    public class ReadResponse
    {
        public string UserName { get; set; }
        public string RoomUuid { get; set; }
        public string MsgUuid { get; set; }
        public long? UnReadCount { get; set; }
        public long? UnReadTotal { get; set; }
    }

    public class DeleteRequest
    {
        public string DeviceId { get; set; }
        public string RoomUuid { get; set; }
        public List<string> ListMsgUuid { get; set; }
    }

    public class DeleteResponse
    {
        public string UserName { get; set; }
        public string DeviceId { get; set; }
        public string RoomUuid { get; set; }
        public long? UnReadCount { get; set; }
        //public long? UnReadTotal { get; set; }
        public List<string> ListMsgUuid { get; set; }
        public MessagesGroupDTO? LastMessenge { get; set; }
    }

    public class PinRequest
    {
        public string RoomUuid { get; set; }
        public List<string> LstMsgUuid { get; set; }
        /// <summary>
        /// 0 = unpin
        /// 1 = pin
        /// </summary>
        public int State { get; set; }
    }

    public class PinResponse
    {
        public string UserName { get; set; }
        public string RoomUuid { get; set; }
        public List<string> LstMsgUuid { get; set; }
        public int State { get; set; }
    }

    public class OnlineStateRequest
    {
        public List<string> LstUser { get; set; }
    }

    public class OnlineStateInfo
    {
        public string UserName { get; set; }
        public bool OnlineState { get; set; }
        public DateTime? TimeOff { get; set; }
    }

    public class OnlineStateResponse
    {
        public List<OnlineStateInfo> LstStateInfo { get; set; }
    }

    public class ForwardMessageRequest
    {
        public string RoomUuid { get; set; }
        public string MsgLineUuid { get; set; }
    }

    public class EditMessageRequest
    {
        public string RoomUuid { get; set; }
        public string MsgLineUuid { get; set; }
        public string Content { get; set; }
    }

    public class LikeMessage
    {
        public string MsgLineUuid { get; set; }
        /// <summary>
        /// Id Emoji
        /// </summary>
        public sbyte Type {  get; set; }
        /// <summary>
        /// 1: Enable - 0: Disable
        /// </summary>
        public sbyte Status {  get; set; }
    }

    public class LikeMessageData : ReactedDTO
    {
        public string MsgLineUuid { get; set; }
        /// <summary>
        /// 1: Enable - 0: Disable
        /// </summary>
        public sbyte Status { get; set; }
    }

    public class LeaveGroupMessage
    {
        public string RoomUuid { get; set; }
    }
    public class JoinGroupMessage
    {
        public string GroupLeader { get; set; }
    }

    public class UpSertMemGroupMessage
    {
        public string GroupUuid { get; set; }
        public string NewMemberUuid { get; set; }
        public sbyte RoleId { get; set; }
        public sbyte Type { get; set; } //1: add 0: remove
    }

    public class BanMessage
    {
        public string RoomUuid { get; set; }
        public string UserName { get; set; }
        public string User {  get; set; }
    }

    public class UnBanMessage
    {
        public string RoomUuid { get; set; }
        public string UserName { get; set; }
        public string User { get; set; }
    }

    public class HideMessageUser
    {
        public string RoomUuid { get; set; }
        public string UserName { get; set; }
    }

    public class DeleteMessageUser
    {
        public string RoomUuid { get; set; }
        public string UserName { get; set; }
    }

    public class BlockMessage
    {
        public string RoomUuid { get; set; }
        public string UserName { get; set; }
    }
    
    public class UnBlockMessage
    {
        public string RoomUuid { get; set; }
        public string UserName { get; set; }
    }
    public class ChangeProfileRequest
    {
        //public string RoomUuid { get; set; }

    }

    public class ChangeProfileResponse
    {
        public string Uuid { get; set; }
        public string? Username { get; set; }
        public string? FullName { get; set; }
        public string? Avatar { get; set; }
    }

}
