using ChatApp.Extensions;
using Microsoft.EntityFrameworkCore;
using static ChatApp.Enums.EnumDatabase;

namespace TWChatAppApiMaster.Utils
{
    public class GroupService
    {
        /// <summary>
        /// Kiểm tra thành viên được gán chức năng không
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="type">Chức năng:
        /// 0: Change group info
        /// 1: Delete message
        /// 2: Ban user
        /// 3: Add member
        /// 4: Lock member
        /// </param>
        /// <returns></returns>
        public static async Task<bool> CheckPermission(string roomUuid, string userName, edTypeGroupPermisson type)
        {
            var context = ServiceExtension.GetDbContext();
            bool result = false;
            try
            {
                var mem = context.RoomMembers.AsNoTracking().First(x => x.RoomUuid == roomUuid && x.UserName == userName);
                switch (type)
                {
                    case edTypeGroupPermisson.CHANGE_GROUP_INFO:
                        if(mem.ChangeGroupInfo == 1) result = true;
                        break;
                    case edTypeGroupPermisson.DELETE_MESSAGE:
                        if (mem.DeleteMessage == 1) result = true;
                        break;
                    case edTypeGroupPermisson.BAN_USER:
                        if (mem.BanUser == 1) result = true;
                        break;
                    case edTypeGroupPermisson.ADD_MEMBER:
                        if (mem.AddMember == 1) result = true;
                        break;
                    case edTypeGroupPermisson.LOCK_MEMBER:
                        if (mem.LockMember == 1) result = true;
                        break;
                    case edTypeGroupPermisson.BLOCK_MEMBER:
                        if (mem.BlockMember == 1) result = true;
                        break;
                    default:
                        break;
                }
            }
            catch 
            {
                return result;
            }
            finally
            {
                context.Dispose();
            }

            return result;
        }
    }
}
