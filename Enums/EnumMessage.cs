using System.ComponentModel;

namespace ChatApp.Enums
{
    public class EnumMessage
    {
        public enum emPostFilterType
        {
            FOLLOWED = 1,
            EVERY_ONE = 2,
        }

        public enum emPostSortType
        {
            NEW = 1,
            HOT = 2,
        }

        public enum emNavigateType
        {
            System = 1,
            LIVE = 2,
            NEW_FOLLOW = 3,
            LIKE_POST = 4,
            COMMENT_POST = 5,
            MESSAGE = 6,
        }

        public enum emLiveUserAction
        {
            [Description("tham gia")]
            JOIN = 0,
            [Description("rời đi")]
            LEAVE = 1,
            [Description("thả tim")]
            LIKE = 2,
            [Description("theo dõi bạn")]
            FOLLOW = 3,
            [Description("tặng quà")]
            GIFT = 4,
            [Description("chat")]
            CHAT = 5,
        }
    }
}
