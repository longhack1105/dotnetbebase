# Socket:
- ChatHandler: Chứa các hàm xử lý logic với database
- WebSocketMiddleware: Thực hiện lắng nghe và gửi đến client
- WebSocketHandler: Mở kết nối + ngắt kết nối ở đây
- Message: chứa các modelData client gửi lên
- ConnectionManager: Xử lý và lưu trữ session nào đang kết nối webSocket
- ClientMessage: ModelData khi client gửi tin nhắn

# Timers:
- AutoDeleteMessageJob: Quét các phòng chat được cài đặt xoá tự động sau đó xoá tin nhắn đạt giới hạn thời gian hiển thị
- TimerProcessMessageDb: Sử lý hàng đợi queue các tài khoản đang không online để gửi FireBase Message Cloud(FCM)

# Middlewares:
- RequestTimingMiddleware: Ghi log thời gian thực thi 1 api
- SecretKeyMiddleware: Bảo mật api với SecretKey(Đang tắt trong program)
- SessionValidationMiddleware: Bảo mật với session

# Databases: Chỉ có các table, không có function
- Account: Tài khoản
- Devices: Thiết bị đăng nhập (1 tài khoản và 1 thiết bị thì chỉ có 1 dữ liệu device)
- FilesInfo: Các file user gửi lên room chat
- Friends: Bạn bè (Chỉ cho phép nhắn tin khi là bạn bè (với accountRole của cả 2 đều là member))
- LogTiming: Log thời gian xử lý của api (lên xoá thường xuyên)
- MessageDelete: Tin nhắn bị xoá 1 phía bởi user
- MessageLike: Tương tác emoji với tin nhắn
- MessagePin: Tin nhắn được ghim trong room chat
- MessageRead: Tin nhắn đọc cuối cùng của user trong room chat
- Messages: Tin nhắn
- Notifications: Thông báo của user(Chưa làm)
- OtpPhone: (Chưa biết để làm gì)
- RegisterAutoDelete: Đăng ký thời gian tự động xoá tin nhắn(Giống Zalo - Tính theo ngày)
- RoomBan: User bị cấm chát trong room
- RoomDelete: Room bị xoá có tin nhắn cuối cùng = lastMessage thì không hiển thị ra danh sách room chat (1 room có thể bị xoá nhiều lần bởi 1 user)
- RoomMembers: Thành viên trong room chat (Với chat 1-1 thì chỉ có role = 3)
- RoomPin: Room chat được user ghim lên đầu danh sách room chat
- Rooms: Room chat
- Session: Quản lý đăng nhập, online
- LoginQRCode: Lưu các request đăng nhập bằng QR. Các request này tồn tại 3 phút. Khi quét đăng nhập thành công thì tạo bảng Session với Session.Uuid = LoginQRCode.Uuid