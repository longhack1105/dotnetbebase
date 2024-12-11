using TWChatAppApiMaster.Databases.ChatApp;

namespace TWChatAppApiMaster.Models.DataInfo
{
    public class RoomAndLastMessageDTO
    {
        public Rooms Room {  get; set; }
        public Messages? LastMessage { get; set; }
    }
}
