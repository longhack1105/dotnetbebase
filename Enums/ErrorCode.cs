using System.ComponentModel;

namespace ChatApp.Enum
{
    public enum ErrorCode
    {
        [Description("Failed")]
        FAILED = -1,
        [Description("Success")]
        SUCCESS = 0,
        [Description("Token Invalid")]
        TOKEN_INVALID = 2,
        [Description("Đã có lỗi xảy ra, bạn vui lòng thử lại")]
        SYSTEM_ERROR = 3,
        [Description("Database failed")]
        DB_FAILED = 4,
        [Description("Thư mục chứa ảnh chưa được cấu hình")]
        FOLDER_IMAGE_NOT_FOUND = 5,
        [Description("Định dạng tập tin không được hỗ trợ")]
        DOES_NOT_SUPPORT_FILE_FORMAT = 6,
        [Description("Not found")]
        NOT_FOUND = 7,
        [Description("Tham số không hợp lệ")]
        INVALID_PARAM = 8,
        [Description("Tài khoản đã tồn tại trên hệ thống")]
        EXISTS = 9,
        [Description("Key cert invalid")]
        INVALID_CERT = 10,
        [Description("Bạn không có quyền thực hiện tác vụ này")]
        PERMISION_DENIED = 11,
        [Description("Tài khoản hoặc mật khẩu không đúng")]
        ACCOUNT_INVALID = 12,
        [Description("Bạn đã có yêu cầu kết bạn với người dùng này. Vui lòng kiểm tra lại")]
        FRIEND_REQUEST_EXISTS = 13,
        [Description("Mật khẩu cũ không chính xác. Vui lòng kiểm tra lại")]
        OLD_PASS_NOT_VALID = 14,
        [Description("Không tìm thấy tài khoản với số điện thoại của bạn. Vui lòng kiểm tra lại")]
        ACCOUNT_NOT_FOUND = 15,
        [Description("Bạn đang là trưởng nhóm nên không thể rời nhóm này")]
        ADMIN_CANNOT_LEAVE_GROUP = 16,
        [Description("Thành viên đã trong nhóm")]
        USER_EXIST_IN_DROUP = 17,

        [Description("Không tìm thấy tài khoản")]
        ACCOUNT_NF = 18,


        [Description("Bad request")]
        BAD_REQUEST = 400,
        [Description("Unauthorization")]
        UNAUTHOR = 401,

        [Description("Tài khoản đang bị khóa")]
        USER_LOCKED = 20,
        [Description("Otp invalid")]
        OTP_INVALID = 21,
        [Description("Otp expired")]
        OTP_EXPIRED = 22,
        [Description("Không thể thao tác tài khoản của bản thân")]
        ACC_IS_MINE = 23,
        [Description("Chọn vai trò")]
        ROLE_ID_BAD_RQ = 24,
        [Description("Nhập tên tài khoản")]
        USER_NAME_RQ = 25,
        [Description("Nhập email")]
        EMAIL_RQ = 26,
        [Description("Email đã tồn tại trên hệ thống")]
        EMAIL_EXISTS = 27,
        [Description("Không thể đồng thời xoá tin nhắn từ 2 khung chat")]
        MSG_ROOM_NOT_UNIQUE = 28,
        [Description("Người dùng đã bị khoá chat trước đó")]
        USER_BANNED = 29,

        [Description("Người dùng không bị khoá chat")]
        USER_NOT_BANNED = 30,

        [Description("QR đăng nhập đã hết hạn")]
        QR_EXPIRED = 31,

        [Description("QR đăng nhập chưa được xác thực")]
        QR_UNAUTHOR = 32,

        [Description("QR đăng nhập đã được xác thực")]
        QR_EIXST = 33,
        [Description("Người dùng đã chặn trước đó")]
        USER_BLOCK = 34,

        [Description("Nhóm không đáp ứng diều kiện")]
        ROOM_DENY = 35,

        [Description("Tài khoản đang bị khóa hoặc không tồn tại")]
        USER_LOCKED_OR_NF = 36,

        [Description("Tạo token thất bại")]
        GEN_TOKEN_FAILED = 37,
    }
}
