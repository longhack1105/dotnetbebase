using System.ComponentModel;

namespace DotnetBeBase.Enums
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
        [Description("Email không hợp lệ")]
        // register
        EMAIL_INVALID = 13,
        [Description("Số điện thoại không hợp lệ")]
        PHONE_INVALID = 14,
        [Description("Mật khẩu không hợp lệ")]
        PASS_INVALID = 15,
        [Description("Tài khoản đã tồn tại")]
        ACCOUNT_EXISTS = 16,

    }
}
