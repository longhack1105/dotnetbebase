using System.ComponentModel;
using static MediaToolkit.Model.Metadata;

namespace ChatApp.Enums
{
    public class EnumDatabase
    {
        public enum edAccountType
        {
            USER = 1,
            IDOL,
            ADMIN,
            MANAGER,
        }

        public enum edAccountState
        {
            [Description("Bị khóa")]
            LOCK = 0,
            [Description("Đang hoạt động")]
            ACTIVE,
        }

        public enum edIsEnable
        {
            [Description("Đã xóa")]
            FALSE,
            [Description("Đang tồn tại")]
            TRUE,
        }

        public enum edSessionState
        {
            [Description("Đang hoạt động")]
            ACTIVE = 1,
            [Description("Đã bị hủy")]
            DESTROYED,
            [Description("Đã đăng xuất")]
            LOGOUT
        }

        public enum edGroupType
        {
            [Description("Chát 1 với 1")]
            CHAT_1_VS_1 = 1,
            [Description("Nhóm chát")]
            GROUP_CHAT,
            [Description("Nhóm xã hội")]
            GROUP_SOCIAL = 3,
            [Description("Nhóm PK")]
            GROUP_PK = 4
        }

        public enum edGroupState
        {
            [Description("Không phải nhóm chát")]
            NOT_A_CHAT_GROUP,
            [Description("Tin nhắn chờ")]
            WATING,
            [Description("Tin nhắn spam")]
            SPAM,
            [Description("Đoạn chát")]
            ALWAYS
        }

        public enum edTable
        {
            ACCOUNT = 1,
            GROUP = 2,
            POST = 3,
            POST_COMMENT = 4,
            LIVE_ROOM = 5,
            NATIONAL = 6,
            GAME = 7,
            CONVERSATIONS = 8,
            EVENT = 9,
            GIFT = 10,
            EMOJI = 11,
            GROUP_LEVEL = 12,
        }

        public enum edRateType
        {
            Like = 1,
            Unlike = 2
        }

        public enum edLiveroomState
        {
            [Description("Đang phát")]
            LIVE_NOW = 1,
            [Description("Đã kết thúc")]
            OVER = 2
        }

        public enum edLiveroomType
        {
            [Description("Live thường")]
            LIVE_NORMALLY = 1,
            [Description("Live nhiều người")]
            LIVE_MULTIPLAYER = 2,
            [Description("Live game")]
            LIVE_GAME = 3,
        }

        public enum edPrivateMode
        {
            [Description("Mọi người")]
            EVERY_ONE = 1,
            [Description("Người theo dõi")]
            FOLLOWER = 2,
            [Description("Bạn bè")]
            FRIENDS = 3,
            [Description("Chỉ mình tôi")]
            PRIVATE = 4,
        }

        public enum edNotificationCategory
        {
            USER_IDOL = 1,
            IDOL = 2,
            MANAGER_IDOL = 3,
            ADMIN = 4,
        }
        public enum edNotificationType
        {
            SYSTEM = 1,
            [Description("[FULL_NAME] đang phát live vào xem ngay")]
            LIVE = 2,
            [Description("[FULL_NAME] đã bắt đầu theo dõi bạn")]
            NEW_FOLLOW = 3,
            [Description("[FULL_NAME] đã yêu thích bài viết của bạn")]
            LIKE_POST = 4,
            [Description("[FULL_NAME] đã bình luận bài viết của bạn")]
            COMMENT_POST = 5,
            [Description("Bạn có tin nhắn mới từ [FULL_NAME]")]
            MESSAGE = 6,
            [Description("Duyệt từ chối idol [NAME]")]
            ACCEPT_REFUSE_IDOL = 7,
            [Description("Idol [NAME] lên cấp")]
            LEVEL_UP = 8,
            [Description("Bạn được mời pk")]
            PK = 9,
        }

        public enum edSetting
        {
            [Description("Cấp độ")]
            LEVEL = 1,
            [Description("Trạng thái hoạt động")]
            ACTIVE = 2,
            [Description("Bình luận")]
            COMMENT = 3,
            [Description("Tin nhắn từ người lạ")]
            MESSAGE = 4,
            [Description("Danh sách người theo dõi")]
            FOLLOW = 5,
        }

        public enum edMode
        {
            [Description("Mọi người")]
            EVERY_ONE = 1,
            [Description("Người theo dõi")]
            FOLLOW = 2,
            [Description("Bạn bè")]
            FRIENDS = 3,
            [Description("Chỉ mình tôi")]
            PRIVATE = 4,
            [Description("Hiển thị")]
            SHOW = 5,
            [Description("Ẩn")]
            HIDE = 6,
            [Description("Cho khép")]
            ALOW = 7,
            [Description("Không cho phép")]
            REFUND = 8,
        }

        public enum edGroupMember
        {
            [Description("Root")]
            ROOT = 0,
            [Description("Nhóm trưởng")]
            OWNER = 1,
            [Description("Thành viên")]
            MEMBER = 2,
        }
        public enum edMessageTypes
        {
            [Description("Mặc định")]
            DEFAUL = 1,
            [Description("Root")]
            ROOT = 2
        }

        public enum edConversationType
        {
            [Description("Chát 1 với 1")]
            CHAT_ONE = 1,
            [Description("Chát nhóm")]
            CHAT_GROUP,
        }

        public enum edIsAccepted
        {
            FALSE,
            TRUE,
        }

        public enum edWalletType
        {
            [Description("Diamond")]
            DIAMOND = 1,
            [Description("Coin")]
            COIN,
        }

        public enum edLiveroomJoinState
        {
            [Description("Đang chờ")]
            WAITING = 1,
            [Description("Đồng ý")]
            AGREE = 2
        }

        public enum edLiveroomJoinType
        {
            [Description("Yêu cầu")]
            REQUEST = 1,
            [Description("Mời")]
            INVITATION = 2,
        }

        public enum edMessageRoot
        {
            [Description("[accountName] đã tạo nhóm")]
            GROUP_CREATED = 1,
            [Description("[accountName] đã đổi tên nhóm thành")]
            NAME_CHANGED = 2,
            [Description("[accountName] đã thay đổi ảnh nhóm")]
            IMAGE_CHANGED = 3,
            [Description("đã có cuộc gọi thoại")]
            HAS_VOICE_CALL = 4,
            [Description("đã có cuộc gọi video")]
            HAS_VIDEO_CALL = 5,
            [Description("[accountName] đã rời nhóm")]
            HAS_LEFT_THE_GROUP = 6,
        }

        public enum eTransaction
        {
            [Description("Nạp kim cương")]
            DEPOSIT = 1,
            [Description("Quy đổi xu thành kim cương")]
            SWAP_COINS,
            [Description("Rút tiền")]
            WITHDRAW,
            [Description("Thu nhập từ Live")]
            INCOME_FROM_LIVE,
            [Description("Thu nhập từ bài đăng")]
            INCOME_FROM_POST,
            [Description("Tặng quà")]
            GIVE_GIFT,
            [Description("Nhận thưởng")]
            REWARD
        }

        public enum edPurpose
        {
            [Description("Ảnh đại diện")]
            Avatar = 1,
            [Description("Ảnh banner")]
            Banner = 2,
            [Description("Hiệu ứng atlas")]
            ATLAS = 3,
            [Description("Hiệu ứng json")]
            JSON = 4,
            [Description("Ảnh gift")]
            GIFT = 5,
            [Description("Ảnh gift animation")]
            ANIMATION = 6,
        }

        public enum edReportState
        {
            [Description("Chưa xử lý")]
            NOT = 1,
            [Description("Đã xử lý")]
            HANDLE = 2,
            [Description("Đã hủy")]
            CANCLE = 1,
        }

        public enum edClassify
        {
            [Description("Quà bình thường")]
            Avatar = 0,
            [Description("Quà đặc biệt(có hiệu ứng)")]
            Banner = 1,
            [Description("Quà miễn phí(dùng cho các hoạt động nhận thưởng)")]
            Free = 2,
        }

        public enum edEffect
        {
            BADGE = 2,
            FRAME_AVATAR = 3,
            JOIN_ROOM = 4,
            CARD = 5,
            [Description("Khung chát")]
            FRAME_CHAT = 6,
            [Description("Bình luận")]
            COMMENT = 7,
        }

        public enum edPostState
        {
            OPEN = 0,
            LOCK = 1,
        }
        

        public enum edRequestFormAccountState
        {
            WAIT = 1,
            ACCEPT = 2,
            REJECT = 3,
        }
        public enum edNotificationAccountState
        {
            UNREAD = 0,
            READ = 1,
        }
        public enum edSortByLevelType
        {
            NO_SORT = 0,
            asc = 1,
            desc = 2,
        }
        public enum edRoomMember
        {
            [Description("Nhóm trưởng")]
            OWNER = 1,
            [Description("Nhóm phó")]
            SUB_OWNER = 2,
            [Description("Thành viên")]
            MEMBER = 3,

        }

        public enum edTypeGroupPermisson
        {
            [Description("Thay đổi thông tin nhóm chát")]
            CHANGE_GROUP_INFO,
            [Description("Xoá tin nhắn")]
            DELETE_MESSAGE,
            [Description("Loại thành viên ra khỏi nhóm")]
            BAN_USER,
            [Description("Thêm thành viên vào nhóm")]
            ADD_MEMBER,
            [Description("Khoá thành viên")]
            LOCK_MEMBER,
            [Description("Chặn thành viên")]
            BLOCK_MEMBER
        }

        public enum edBlockState
        {
            UNBLOCK = 0,
            BLOCK = 1,
        }

        public enum edFriendRequestSendStatus
        {
            [Description("Nhận")]
            RECEIVE = 0,
            [Description("Gửi")]
            SEND,
        }

        public enum edPinState
        {
            [Description("Nhận")]
            RECEIVE = 0,
            [Description("Gửi")]
            SEND,
        }

        // 1: Text - 2: Link - 3: Image - 4: Video - 5: Audio - 6: Void Call - 7: Video Call
        public enum edContentType
        {
            [Description("Text")]
            TEXT = 1,
            [Description("Link")]
            LINK,
            [Description("Image")]
            IMAGE,
            [Description("Video")]
            VIDEO,
            [Description("Audio")]
            AUDIO,
            [Description("Void Call")]
            VOID_CALL,
            [Description("Video Call")]
            VIDEO_CALL,

        }
    }
}
