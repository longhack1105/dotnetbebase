using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TWChatAppApiMaster.Databases.ChatApp
{
    public partial class DBContext : DbContext
    {
        public DBContext()
        {
        }

        public DBContext(DbContextOptions<DBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Account { get; set; } = null!;
        public virtual DbSet<Devices> Devices { get; set; } = null!;
        public virtual DbSet<FilesInfo> FilesInfo { get; set; } = null!;
        public virtual DbSet<Friends> Friends { get; set; } = null!;
        public virtual DbSet<LogTiming> LogTiming { get; set; } = null!;
        public virtual DbSet<LoginQrCode> LoginQrCode { get; set; } = null!;
        public virtual DbSet<MessageDelete> MessageDelete { get; set; } = null!;
        public virtual DbSet<MessageLike> MessageLike { get; set; } = null!;
        public virtual DbSet<MessagePin> MessagePin { get; set; } = null!;
        public virtual DbSet<MessageRead> MessageRead { get; set; } = null!;
        public virtual DbSet<Messages> Messages { get; set; } = null!;
        public virtual DbSet<Notifications> Notifications { get; set; } = null!;
        public virtual DbSet<OtpPhone> OtpPhone { get; set; } = null!;
        public virtual DbSet<RegisterAutoDelete> RegisterAutoDelete { get; set; } = null!;
        public virtual DbSet<RoomBan> RoomBan { get; set; } = null!;
        public virtual DbSet<RoomBlock> RoomBlock { get; set; } = null!;
        public virtual DbSet<RoomDelete> RoomDelete { get; set; } = null!;
        public virtual DbSet<RoomMembers> RoomMembers { get; set; } = null!;
        public virtual DbSet<RoomPin> RoomPin { get; set; } = null!;
        public virtual DbSet<RoomRecent> RoomRecent { get; set; } = null!;
        public virtual DbSet<Rooms> Rooms { get; set; } = null!;
        public virtual DbSet<Session> Session { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("latin1_swedish_ci")
                .HasCharSet("latin1");

            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("account");

                entity.HasCharSet("utf8mb4")
                    .UseCollation("utf8mb4_unicode_520_ci");

                entity.HasIndex(e => e.UserName, "user_name")
                    .IsUnique();

                entity.HasIndex(e => e.Uuid, "uuid")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("int(10)")
                    .HasColumnName("id");

                entity.Property(e => e.ActiveState)
                    .HasColumnType("tinyint(4)")
                    .HasColumnName("active_state")
                    .HasDefaultValueSql("'1'")
                    .HasComment("0: lock 1: active");

                entity.Property(e => e.Avatar)
                    .HasMaxLength(255)
                    .HasColumnName("avatar");

                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .HasColumnName("email");

                entity.Property(e => e.FullName)
                    .HasMaxLength(255)
                    .HasColumnName("full_name");

                entity.Property(e => e.IsEnable)
                    .HasColumnType("bit(1)")
                    .HasColumnName("is_enable")
                    .HasDefaultValueSql("b'1'");

                entity.Property(e => e.LastSeen)
                    .HasColumnType("timestamp")
                    .HasColumnName("last_seen")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.LastUpdated)
                    .HasColumnType("timestamp")
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnName("last_updated")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.PassWord)
                    .HasMaxLength(255)
                    .HasColumnName("pass_word");

                entity.Property(e => e.ReceiveNotifyStatus)
                    .HasColumnType("tinyint(4)")
                    .HasColumnName("receive_notify_status")
                    .HasDefaultValueSql("'1'")
                    .HasComment("0: not register 1: receive");

                entity.Property(e => e.RoleId)
                    .HasColumnType("tinyint(4)")
                    .HasColumnName("role_id")
                    .HasDefaultValueSql("'0'")
                    .HasComment("0: normal - 1: leader - 2: admin");

                entity.Property(e => e.Status)
                    .HasColumnType("tinyint(2)")
                    .HasColumnName("status")
                    .HasComment("0: Pending - 1: Active - 2:Deactive");

                entity.Property(e => e.TimeCreated)
                    .HasColumnType("timestamp")
                    .HasColumnName("time_created")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.UserName)
                    .HasMaxLength(50)
                    .HasColumnName("user_name");

                entity.Property(e => e.Uuid)
                    .HasMaxLength(36)
                    .HasColumnName("uuid")
                    .HasDefaultValueSql("uuid()")
                    .IsFixedLength();
            });

            modelBuilder.Entity<Devices>(entity =>
            {
                entity.ToTable("devices");

                entity.HasCharSet("utf8mb4")
                    .UseCollation("utf8mb4_unicode_520_ci");

                entity.HasIndex(e => e.UserName, "dv_user_ref");

                entity.Property(e => e.Id)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("id");

                entity.Property(e => e.Address)
                    .HasMaxLength(255)
                    .HasColumnName("address");

                entity.Property(e => e.DeviceId)
                    .HasMaxLength(50)
                    .HasColumnName("device_id");

                entity.Property(e => e.DeviceName)
                    .HasMaxLength(50)
                    .HasColumnName("device_name");

                entity.Property(e => e.Ip)
                    .HasMaxLength(100)
                    .HasColumnName("ip");

                entity.Property(e => e.LastUsed)
                    .HasColumnType("timestamp")
                    .HasColumnName("last_used")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.Os)
                    .HasMaxLength(10)
                    .HasColumnName("os");

                entity.Property(e => e.Status)
                    .HasColumnType("tinyint(4)")
                    .HasColumnName("status")
                    .HasDefaultValueSql("'1'")
                    .HasComment("1: Using - 2: Inacctive");

                entity.Property(e => e.TimeCreated)
                    .HasColumnType("timestamp")
                    .HasColumnName("time_created")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.UserName)
                    .HasMaxLength(36)
                    .HasColumnName("user_name");

                entity.HasOne(d => d.UserNameNavigation)
                    .WithMany(p => p.Devices)
                    .HasPrincipalKey(p => p.UserName)
                    .HasForeignKey(d => d.UserName)
                    .HasConstraintName("devices_ibfk_1");
            });

            modelBuilder.Entity<FilesInfo>(entity =>
            {
                entity.ToTable("files_info");

                entity.HasCharSet("utf8mb4")
                    .UseCollation("utf8mb4_unicode_520_ci");

                entity.HasIndex(e => e.UserUpload, "owner_uuid");

                entity.HasIndex(e => e.Uuid, "uuid")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("id");

                entity.Property(e => e.FileName)
                    .HasMaxLength(255)
                    .HasColumnName("file_name");

                entity.Property(e => e.Path)
                    .HasMaxLength(255)
                    .HasColumnName("path");

                entity.Property(e => e.TimeCreated)
                    .HasColumnType("timestamp")
                    .HasColumnName("time_created")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.UserUpload)
                    .HasMaxLength(36)
                    .HasColumnName("user_upload")
                    .IsFixedLength();

                entity.Property(e => e.Uuid)
                    .HasMaxLength(36)
                    .HasColumnName("uuid")
                    .IsFixedLength();
            });

            modelBuilder.Entity<Friends>(entity =>
            {
                entity.ToTable("friends");

                entity.HasComment("Bạn bè")
                    .HasCharSet("utf8mb4")
                    .UseCollation("utf8mb4_unicode_520_ci");

                entity.HasIndex(e => e.UserReceiver, "frd_usr_rcv");

                entity.HasIndex(e => e.UserSent, "frd_usr_ref");

                entity.HasIndex(e => e.Uuid, "uuid_unq")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("int(10)")
                    .HasColumnName("id");

                entity.Property(e => e.LastUpdated)
                    .HasColumnType("timestamp")
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnName("last_updated")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.Status)
                    .HasColumnType("tinyint(2)")
                    .HasColumnName("status")
                    .HasDefaultValueSql("'2'")
                    .HasComment("1: ko bạn bè; \r\n2: Chờ xác nhận; \r\n3: Bạn bè; ");

                entity.Property(e => e.TimeCreated)
                    .HasColumnType("timestamp")
                    .HasColumnName("time_created")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.Type)
                    .HasColumnType("tinyint(2)")
                    .HasColumnName("type")
                    .HasComment("1: Friend - 2: Blocked");

                entity.Property(e => e.UserReceiver)
                    .HasMaxLength(50)
                    .HasColumnName("user_receiver")
                    .HasComment("Người nhận lời mời");

                entity.Property(e => e.UserSent)
                    .HasMaxLength(50)
                    .HasColumnName("user_sent")
                    .HasComment("Người gửi lời mời");

                entity.Property(e => e.Uuid)
                    .HasMaxLength(36)
                    .HasColumnName("uuid")
                    .IsFixedLength();

                entity.HasOne(d => d.UserReceiverNavigation)
                    .WithMany(p => p.FriendsUserReceiverNavigation)
                    .HasPrincipalKey(p => p.UserName)
                    .HasForeignKey(d => d.UserReceiver)
                    .HasConstraintName("friends_ibfk_1");

                entity.HasOne(d => d.UserSentNavigation)
                    .WithMany(p => p.FriendsUserSentNavigation)
                    .HasPrincipalKey(p => p.UserName)
                    .HasForeignKey(d => d.UserSent)
                    .HasConstraintName("friends_ibfk_2");
            });

            modelBuilder.Entity<LogTiming>(entity =>
            {
                entity.ToTable("log_timing");

                entity.HasCharSet("utf8mb4")
                    .UseCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.Id)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("id");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.Request).HasColumnName("request");

                entity.Property(e => e.Response).HasColumnName("response");

                entity.Property(e => e.TimeCreated)
                    .HasColumnType("timestamp")
                    .HasColumnName("time_created")
                    .HasDefaultValueSql("(utc_timestamp() + interval 7 hour)");

                entity.Property(e => e.TimeHandle)
                    .HasColumnType("int(11)")
                    .HasColumnName("time_handle");
            });

            modelBuilder.Entity<LoginQrCode>(entity =>
            {
                entity.ToTable("login_qr_code");

                entity.HasCharSet("utf8mb4")
                    .UseCollation("utf8mb4_unicode_ci");

                entity.HasIndex(e => e.Uuid, "uuid")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("id");

                entity.Property(e => e.Address)
                    .HasMaxLength(255)
                    .HasColumnName("address")
                    .UseCollation("utf8mb4_unicode_520_ci");

                entity.Property(e => e.DeviceId)
                    .HasMaxLength(50)
                    .HasColumnName("device_id")
                    .UseCollation("utf8mb4_unicode_520_ci");

                entity.Property(e => e.DeviceName)
                    .HasMaxLength(50)
                    .HasColumnName("device_name")
                    .UseCollation("utf8mb4_unicode_520_ci");

                entity.Property(e => e.FcmToken)
                    .HasMaxLength(255)
                    .HasColumnName("fcm_token");

                entity.Property(e => e.Ip)
                    .HasMaxLength(100)
                    .HasColumnName("ip")
                    .UseCollation("utf8mb4_unicode_520_ci");

                entity.Property(e => e.Os)
                    .HasMaxLength(10)
                    .HasColumnName("os")
                    .UseCollation("utf8mb4_unicode_520_ci");

                entity.Property(e => e.TimeCreated)
                    .HasColumnType("timestamp")
                    .HasColumnName("time_created")
                    .HasDefaultValueSql("utc_timestamp()");

                entity.Property(e => e.TimeExpired)
                    .HasColumnType("timestamp")
                    .HasColumnName("time_expired")
                    .HasDefaultValueSql("(utc_timestamp() + interval 3 minute)");

                entity.Property(e => e.Uuid)
                    .HasMaxLength(36)
                    .HasColumnName("uuid")
                    .IsFixedLength()
                    .HasComment("key tạo QR code login (bằng với uuid session)")
                    .UseCollation("utf8mb4_unicode_520_ci");
            });

            modelBuilder.Entity<MessageDelete>(entity =>
            {
                entity.ToTable("message_delete");

                entity.HasCharSet("utf8mb4")
                    .UseCollation("utf8mb4_unicode_520_ci");

                entity.HasIndex(e => e.MessageUuid, "message_line_uuid");

                entity.HasIndex(e => e.UserName, "user_name");

                entity.Property(e => e.Id)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("id");

                entity.Property(e => e.MessageUuid)
                    .HasMaxLength(36)
                    .HasColumnName("message_uuid")
                    .IsFixedLength();

                entity.Property(e => e.TimeCreated)
                    .HasColumnType("timestamp")
                    .HasColumnName("time_created")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.UserName)
                    .HasMaxLength(50)
                    .HasColumnName("user_name");

                entity.HasOne(d => d.MessageUu)
                    .WithMany(p => p.MessageDelete)
                    .HasPrincipalKey(p => p.Uuid)
                    .HasForeignKey(d => d.MessageUuid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("message_delete_ibfk_1");
            });

            modelBuilder.Entity<MessageLike>(entity =>
            {
                entity.ToTable("message_like");

                entity.HasComment("Like tin nhắn")
                    .HasCharSet("utf8mb4")
                    .UseCollation("utf8mb4_unicode_520_ci");

                entity.HasIndex(e => e.MessageUuid, "msg_line_uid_ref");

                entity.HasIndex(e => e.UserName, "msg_usr_ref");

                entity.Property(e => e.Id)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("id");

                entity.Property(e => e.MessageUuid)
                    .HasMaxLength(36)
                    .HasColumnName("message_uuid")
                    .IsFixedLength();

                entity.Property(e => e.Status)
                    .HasColumnType("tinyint(2)")
                    .HasColumnName("status")
                    .HasDefaultValueSql("'1'")
                    .HasComment("1: Enable - 0: Disable");

                entity.Property(e => e.TimeCreated)
                    .HasColumnType("timestamp")
                    .HasColumnName("time_created")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.Type)
                    .HasColumnType("tinyint(4)")
                    .HasColumnName("type")
                    .HasComment("1: Tim - 2: Like - 3: Cười ...");

                entity.Property(e => e.UserName)
                    .HasMaxLength(50)
                    .HasColumnName("user_name");

                entity.HasOne(d => d.MessageUu)
                    .WithMany(p => p.MessageLike)
                    .HasPrincipalKey(p => p.Uuid)
                    .HasForeignKey(d => d.MessageUuid)
                    .HasConstraintName("message_like_ibfk_1");

                entity.HasOne(d => d.UserNameNavigation)
                    .WithMany(p => p.MessageLike)
                    .HasPrincipalKey(p => p.UserName)
                    .HasForeignKey(d => d.UserName)
                    .HasConstraintName("message_like_ibfk_2");
            });

            modelBuilder.Entity<MessagePin>(entity =>
            {
                entity.ToTable("message_pin");

                entity.HasComment("Ghim tin nhắn")
                    .HasCharSet("utf8mb4")
                    .UseCollation("utf8mb4_unicode_520_ci");

                entity.HasIndex(e => e.MessageUuid, "msg_line_uuid");

                entity.HasIndex(e => e.RoomUuid, "room_uuid");

                entity.HasIndex(e => e.UserName, "user_name");

                entity.Property(e => e.Id)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("id");

                entity.Property(e => e.MessageUuid)
                    .HasMaxLength(36)
                    .HasColumnName("message_uuid")
                    .IsFixedLength();

                entity.Property(e => e.RoomUuid)
                    .HasMaxLength(36)
                    .HasColumnName("room_uuid")
                    .IsFixedLength();

                entity.Property(e => e.State)
                    .HasColumnType("tinyint(2)")
                    .HasColumnName("state")
                    .HasDefaultValueSql("'1'")
                    .HasComment("0: unpin 1: pin");

                entity.Property(e => e.TimePin)
                    .HasColumnType("timestamp")
                    .HasColumnName("time_pin")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.UserName)
                    .HasMaxLength(50)
                    .HasColumnName("user_name");

                entity.HasOne(d => d.MessageUu)
                    .WithMany(p => p.MessagePin)
                    .HasPrincipalKey(p => p.Uuid)
                    .HasForeignKey(d => d.MessageUuid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("message_pin_ibfk_2");

                entity.HasOne(d => d.RoomUu)
                    .WithMany(p => p.MessagePin)
                    .HasPrincipalKey(p => p.Uuid)
                    .HasForeignKey(d => d.RoomUuid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("message_pin_ibfk_1");

                entity.HasOne(d => d.UserNameNavigation)
                    .WithMany(p => p.MessagePin)
                    .HasPrincipalKey(p => p.UserName)
                    .HasForeignKey(d => d.UserName)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("message_pin_ibfk_3");
            });

            modelBuilder.Entity<MessageRead>(entity =>
            {
                entity.ToTable("message_read");

                entity.HasComment("Tin nhắn đã đọc")
                    .HasCharSet("utf8mb4")
                    .UseCollation("utf8mb4_unicode_520_ci");

                entity.HasIndex(e => e.LastMessageId, "msg_uid_ref");

                entity.HasIndex(e => new { e.LastMessageId, e.UserName }, "msl_user_unq")
                    .IsUnique();

                entity.HasIndex(e => new { e.RoomUuid, e.UserName }, "room_uuid_2");

                entity.HasIndex(e => e.UserName, "user_name");

                entity.Property(e => e.Id)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("id");

                entity.Property(e => e.LastMessageId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("last_message_id")
                    .HasComment("Tin nhắn đọc cuối cùng");

                entity.Property(e => e.RoomUuid).HasColumnName("room_uuid");

                entity.Property(e => e.TimeRead)
                    .HasColumnType("timestamp")
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnName("time_read")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.UserName)
                    .HasMaxLength(50)
                    .HasColumnName("user_name");

                entity.HasOne(d => d.LastMessage)
                    .WithMany(p => p.MessageRead)
                    .HasForeignKey(d => d.LastMessageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("message_read_ibfk_4");

                entity.HasOne(d => d.RoomUu)
                    .WithMany(p => p.MessageRead)
                    .HasPrincipalKey(p => p.Uuid)
                    .HasForeignKey(d => d.RoomUuid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("message_read_ibfk_1");

                entity.HasOne(d => d.UserNameNavigation)
                    .WithMany(p => p.MessageRead)
                    .HasPrincipalKey(p => p.UserName)
                    .HasForeignKey(d => d.UserName)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("message_read_ibfk_5");
            });

            modelBuilder.Entity<Messages>(entity =>
            {
                entity.ToTable("messages");

                entity.HasCharSet("utf8mb4")
                    .UseCollation("utf8mb4_unicode_520_ci");

                entity.HasIndex(e => e.RoomUuid, "msgl_uid_ref");

                entity.HasIndex(e => e.UserSent, "msgl_usr_ref");

                entity.HasIndex(e => e.ReplyMessageUuid, "reply_msg_uid_ref");

                entity.HasIndex(e => e.Status, "status");

                entity.HasIndex(e => e.Uuid, "uuid")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("id");

                entity.Property(e => e.Content)
                    .HasColumnName("content")
                    .UseCollation("utf8mb4_bin");

                entity.Property(e => e.ContentType)
                    .HasColumnType("tinyint(4)")
                    .HasColumnName("content_type")
                    .HasComment("1: Text - 2: Link - 3: Image - 4: Video - 5: Audio - 6: Void Call - 7: Video Call");

                entity.Property(e => e.FileInfo)
                    .HasColumnName("file_info")
                    .HasComment("ảnh thumb video do client gửi");

                entity.Property(e => e.ForwardFrom)
                    .HasColumnType("text")
                    .HasColumnName("forward_from");

                entity.Property(e => e.LanguageCode)
                    .HasMaxLength(255)
                    .HasColumnName("language_code");

                entity.Property(e => e.LastEdited)
                    .HasColumnType("timestamp")
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnName("last_edited")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.ReplyMessageUuid)
                    .HasMaxLength(36)
                    .HasColumnName("reply_message_uuid")
                    .IsFixedLength();

                entity.Property(e => e.RoomUuid)
                    .HasMaxLength(36)
                    .HasColumnName("room_uuid")
                    .IsFixedLength();

                entity.Property(e => e.Status)
                    .HasColumnType("tinyint(2)")
                    .HasColumnName("status")
                    .HasComment("1: Normal; \r\n2: editted; \r\n3: hide with member; \r\n4: deleted;");

                entity.Property(e => e.TimeCreated)
                    .HasColumnType("timestamp")
                    .HasColumnName("time_created")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.UserSent)
                    .HasMaxLength(50)
                    .HasColumnName("user_sent");

                entity.Property(e => e.Uuid)
                    .HasMaxLength(36)
                    .HasColumnName("uuid")
                    .HasDefaultValueSql("uuid()")
                    .IsFixedLength();

                entity.HasOne(d => d.ReplyMessageUu)
                    .WithMany(p => p.InverseReplyMessageUu)
                    .HasPrincipalKey(p => p.Uuid)
                    .HasForeignKey(d => d.ReplyMessageUuid)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("messages_ibfk_3");

                entity.HasOne(d => d.RoomUu)
                    .WithMany(p => p.Messages)
                    .HasPrincipalKey(p => p.Uuid)
                    .HasForeignKey(d => d.RoomUuid)
                    .HasConstraintName("messages_ibfk_1");

                entity.HasOne(d => d.UserSentNavigation)
                    .WithMany(p => p.Messages)
                    .HasPrincipalKey(p => p.UserName)
                    .HasForeignKey(d => d.UserSent)
                    .HasConstraintName("messages_ibfk_2");
            });

            modelBuilder.Entity<Notifications>(entity =>
            {
                entity.ToTable("notifications");

                entity.HasCharSet("utf8mb4")
                    .UseCollation("utf8mb4_unicode_520_ci");

                entity.HasIndex(e => e.UserName, "ntf_usr_ref");

                entity.Property(e => e.Id)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("id");

                entity.Property(e => e.ActionId)
                    .HasColumnType("int(4)")
                    .HasColumnName("action_id");

                entity.Property(e => e.Content)
                    .HasMaxLength(255)
                    .HasColumnName("content");

                entity.Property(e => e.Status)
                    .HasColumnType("tinyint(4)")
                    .HasColumnName("status")
                    .HasComment("0: Chưa xem - 1: Đã xem");

                entity.Property(e => e.TimeCreated)
                    .HasColumnType("timestamp")
                    .HasColumnName("time_created")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.Title)
                    .HasMaxLength(50)
                    .HasColumnName("title");

                entity.Property(e => e.Type)
                    .HasColumnType("tinyint(4)")
                    .HasColumnName("type");

                entity.Property(e => e.UserName)
                    .HasMaxLength(50)
                    .HasColumnName("user_name");

                entity.Property(e => e.Uuid)
                    .HasMaxLength(36)
                    .HasColumnName("uuid")
                    .IsFixedLength();

                entity.HasOne(d => d.UserNameNavigation)
                    .WithMany(p => p.Notifications)
                    .HasPrincipalKey(p => p.UserName)
                    .HasForeignKey(d => d.UserName)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("notifications_ibfk_1");
            });

            modelBuilder.Entity<OtpPhone>(entity =>
            {
                entity.ToTable("otp_phone");

                entity.HasCharSet("utf8mb4")
                    .UseCollation("utf8mb4_unicode_520_ci");

                entity.Property(e => e.Id)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("id");

                entity.Property(e => e.Action)
                    .HasColumnType("tinyint(4)")
                    .HasColumnName("action")
                    .HasComment("1: Register, 2: Fogot password, 3: Change password");

                entity.Property(e => e.Note)
                    .HasMaxLength(255)
                    .HasColumnName("note");

                entity.Property(e => e.Otp)
                    .HasMaxLength(20)
                    .HasColumnName("otp");

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(50)
                    .HasColumnName("phone_number");

                entity.Property(e => e.Status)
                    .HasColumnType("tinyint(4)")
                    .HasColumnName("status")
                    .HasDefaultValueSql("'1'")
                    .HasComment("1: Active, 0: Off");

                entity.Property(e => e.TimeCreated)
                    .HasColumnType("timestamp")
                    .HasColumnName("time_created")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.TimeExpired)
                    .HasColumnType("timestamp")
                    .HasColumnName("time_expired");

                entity.Property(e => e.UserUsed)
                    .HasMaxLength(50)
                    .HasColumnName("user_used");
            });

            modelBuilder.Entity<RegisterAutoDelete>(entity =>
            {
                entity.ToTable("register_auto_delete");

                entity.HasComment("Đăng ký tự động xoá tin nhắn")
                    .HasCharSet("utf8mb4")
                    .UseCollation("utf8mb4_general_ci");

                entity.HasIndex(e => e.RoomUuid, "room_uuid");

                entity.HasIndex(e => e.UserName, "user_name");

                entity.Property(e => e.Id)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("id");

                entity.Property(e => e.LastTimeDelete)
                    .HasColumnType("datetime")
                    .HasColumnName("last_time_delete");

                entity.Property(e => e.PeriodTime)
                    .HasColumnType("int(11)")
                    .HasColumnName("period_time");

                entity.Property(e => e.RoomUuid)
                    .HasMaxLength(36)
                    .HasColumnName("room_uuid")
                    .IsFixedLength()
                    .UseCollation("utf8mb4_unicode_520_ci");

                entity.Property(e => e.UserName)
                    .HasMaxLength(50)
                    .HasColumnName("user_name")
                    .UseCollation("utf8mb4_unicode_520_ci");

                entity.HasOne(d => d.RoomUu)
                    .WithMany(p => p.RegisterAutoDelete)
                    .HasPrincipalKey(p => p.Uuid)
                    .HasForeignKey(d => d.RoomUuid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("register_auto_delete_ibfk_2");

                entity.HasOne(d => d.UserNameNavigation)
                    .WithMany(p => p.RegisterAutoDelete)
                    .HasPrincipalKey(p => p.UserName)
                    .HasForeignKey(d => d.UserName)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("register_auto_delete_ibfk_1");
            });

            modelBuilder.Entity<RoomBan>(entity =>
            {
                entity.ToTable("room_ban");

                entity.HasComment("Cấm chat trong nhóm")
                    .HasCharSet("utf8mb4")
                    .UseCollation("utf8mb4_unicode_520_ci");

                entity.HasIndex(e => e.RoomUuid, "room_uuid");

                entity.HasIndex(e => e.UserName, "user_name");

                entity.Property(e => e.Id)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("id");

                entity.Property(e => e.RoomUuid)
                    .HasMaxLength(36)
                    .HasColumnName("room_uuid")
                    .IsFixedLength();

                entity.Property(e => e.State)
                    .HasColumnType("tinyint(2)")
                    .HasColumnName("state")
                    .HasDefaultValueSql("'1'")
                    .HasComment("0 = unbaned; \r\n1 = banned; \r\n");

                entity.Property(e => e.TimeCreated)
                    .HasColumnType("timestamp")
                    .HasColumnName("time_created")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.TimeUpdated)
                    .HasColumnType("timestamp")
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnName("time_updated");

                entity.Property(e => e.UserName)
                    .HasMaxLength(50)
                    .HasColumnName("user_name");

                entity.HasOne(d => d.RoomUu)
                    .WithMany(p => p.RoomBan)
                    .HasPrincipalKey(p => p.Uuid)
                    .HasForeignKey(d => d.RoomUuid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("room_ban_ibfk_2");

                entity.HasOne(d => d.UserNameNavigation)
                    .WithMany(p => p.RoomBan)
                    .HasPrincipalKey(p => p.UserName)
                    .HasForeignKey(d => d.UserName)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("room_ban_ibfk_1");
            });

            modelBuilder.Entity<RoomBlock>(entity =>
            {
                entity.ToTable("room_block");

                entity.HasComment("Chặn người dùng trong nhóm")
                    .HasCharSet("utf8mb4")
                    .UseCollation("utf8mb4_unicode_520_ci");

                entity.HasIndex(e => e.RoomUuid, "room_uuid");

                entity.HasIndex(e => e.UserName, "user_name");

                entity.Property(e => e.Id)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("id");

                entity.Property(e => e.RoomUuid)
                    .HasMaxLength(36)
                    .HasColumnName("room_uuid")
                    .IsFixedLength();

                entity.Property(e => e.State)
                    .HasColumnType("tinyint(2)")
                    .HasColumnName("state")
                    .HasDefaultValueSql("'1'")
                    .HasComment("0 = unblocked; \r\n1 = blocked; \r\n");

                entity.Property(e => e.TimeCreated)
                    .HasColumnType("timestamp")
                    .HasColumnName("time_created")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.TimeUpdated)
                    .HasColumnType("timestamp")
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnName("time_updated");

                entity.Property(e => e.UserName)
                    .HasMaxLength(50)
                    .HasColumnName("user_name");

                entity.HasOne(d => d.RoomUu)
                    .WithMany(p => p.RoomBlock)
                    .HasPrincipalKey(p => p.Uuid)
                    .HasForeignKey(d => d.RoomUuid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("room_block_ibfk_2");

                entity.HasOne(d => d.UserNameNavigation)
                    .WithMany(p => p.RoomBlock)
                    .HasPrincipalKey(p => p.UserName)
                    .HasForeignKey(d => d.UserName)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("room_block_ibfk_1");
            });

            modelBuilder.Entity<RoomDelete>(entity =>
            {
                entity.ToTable("room_delete");

                entity.HasCharSet("utf8mb4")
                    .UseCollation("utf8mb4_unicode_520_ci");

                entity.HasIndex(e => e.LastMessageId, "last_message_id");

                entity.HasIndex(e => e.RoomUuid, "room_uuid");

                entity.HasIndex(e => e.UserName, "user_name");

                entity.Property(e => e.Id)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("id");

                entity.Property(e => e.LastMessageId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("last_message_id");

                entity.Property(e => e.RoomUuid)
                    .HasMaxLength(36)
                    .HasColumnName("room_uuid")
                    .IsFixedLength();

                entity.Property(e => e.TimeCreated)
                    .HasColumnType("timestamp")
                    .HasColumnName("time_created")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.UserName)
                    .HasMaxLength(50)
                    .HasColumnName("user_name");

                entity.HasOne(d => d.LastMessage)
                    .WithMany(p => p.RoomDelete)
                    .HasForeignKey(d => d.LastMessageId)
                    .HasConstraintName("room_delete_ibfk_3");

                entity.HasOne(d => d.RoomUu)
                    .WithMany(p => p.RoomDelete)
                    .HasPrincipalKey(p => p.Uuid)
                    .HasForeignKey(d => d.RoomUuid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("room_delete_ibfk_1");

                entity.HasOne(d => d.UserNameNavigation)
                    .WithMany(p => p.RoomDelete)
                    .HasPrincipalKey(p => p.UserName)
                    .HasForeignKey(d => d.UserName)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("room_delete_ibfk_2");
            });

            modelBuilder.Entity<RoomMembers>(entity =>
            {
                entity.ToTable("room_members");

                entity.HasCharSet("utf8mb4")
                    .UseCollation("utf8mb4_unicode_520_ci");

                entity.HasIndex(e => e.RoomUuid, "room_uuid");

                entity.HasIndex(e => e.UserName, "user_name");

                entity.Property(e => e.Id)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("id");

                entity.Property(e => e.AddMember)
                    .HasColumnType("bit(1)")
                    .HasColumnName("add_member")
                    .HasDefaultValueSql("b'0'")
                    .HasComment("chức năng thêm thành viên");

                entity.Property(e => e.BanUser)
                    .HasColumnType("bit(1)")
                    .HasColumnName("ban_user")
                    .HasDefaultValueSql("b'0'")
                    .HasComment("chức năng ban thành viên");

                entity.Property(e => e.BlockMember)
                    .HasColumnType("bit(1)")
                    .HasColumnName("block_member")
                    .HasDefaultValueSql("b'0'")
                    .HasComment("chức năng chặn/mở chặn thành viên");

                entity.Property(e => e.CanMakeFriend)
                    .HasColumnType("bit(1)")
                    .HasColumnName("can_make_friend")
                    .HasDefaultValueSql("b'0'")
                    .HasComment("0 = Không cho phép kết bạn; \r\n1 = Được phép kết bạn;");

                entity.Property(e => e.ChangeGroupInfo)
                    .HasColumnType("bit(1)")
                    .HasColumnName("change_group_info")
                    .HasDefaultValueSql("b'0'")
                    .HasComment("chức năng thay đổi thông tin nhóm");

                entity.Property(e => e.DeleteMessage)
                    .HasColumnType("bit(1)")
                    .HasColumnName("delete_message")
                    .HasDefaultValueSql("b'0'")
                    .HasComment("chức năng xoá tin nhắn");

                entity.Property(e => e.InRoom)
                    .HasColumnType("bit(1)")
                    .HasColumnName("in_room")
                    .HasDefaultValueSql("b'1'")
                    .HasComment("0 = leaved; \r\n1 = inroom;  ");

                entity.Property(e => e.LockMember)
                    .HasColumnType("bit(1)")
                    .HasColumnName("lock_member")
                    .HasDefaultValueSql("b'0'")
                    .HasComment("chức năng khoá/mở thành viên");

                entity.Property(e => e.RoleId)
                    .HasColumnType("tinyint(2)")
                    .HasColumnName("role_id")
                    .HasDefaultValueSql("'3'")
                    .HasComment("1 = Trưởng nhóm; \r\n2 = Phó nhóm; \r\n3 = Thành viên thường; ");

                entity.Property(e => e.RoomUuid)
                    .HasMaxLength(36)
                    .HasColumnName("room_uuid")
                    .IsFixedLength();

                entity.Property(e => e.UserName)
                    .HasMaxLength(50)
                    .HasColumnName("user_name");

                entity.HasOne(d => d.RoomUu)
                    .WithMany(p => p.RoomMembers)
                    .HasPrincipalKey(p => p.Uuid)
                    .HasForeignKey(d => d.RoomUuid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("room_members_ibfk_2");

                entity.HasOne(d => d.UserNameNavigation)
                    .WithMany(p => p.RoomMembers)
                    .HasPrincipalKey(p => p.UserName)
                    .HasForeignKey(d => d.UserName)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("room_members_ibfk_1");
            });

            modelBuilder.Entity<RoomPin>(entity =>
            {
                entity.ToTable("room_pin");

                entity.HasComment("Ghim cuộc trò chuyện")
                    .HasCharSet("utf8mb4")
                    .UseCollation("utf8mb4_general_ci");

                entity.HasIndex(e => e.RoomUuid, "room_uuid");

                entity.HasIndex(e => e.UserName, "user_name");

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");

                entity.Property(e => e.RoomUuid)
                    .HasColumnName("room_uuid")
                    .UseCollation("utf8mb4_unicode_520_ci");

                entity.Property(e => e.State)
                    .HasColumnType("tinyint(4)")
                    .HasColumnName("state")
                    .HasDefaultValueSql("'1'")
                    .HasComment("0: unpin  1:pin");

                entity.Property(e => e.TimePin)
                    .HasColumnType("timestamp")
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnName("time_pin")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.UserName)
                    .HasColumnName("user_name")
                    .UseCollation("utf8mb4_unicode_520_ci");

                entity.HasOne(d => d.RoomUu)
                    .WithMany(p => p.RoomPin)
                    .HasPrincipalKey(p => p.Uuid)
                    .HasForeignKey(d => d.RoomUuid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("room_pin_ibfk_1");

                entity.HasOne(d => d.UserNameNavigation)
                    .WithMany(p => p.RoomPin)
                    .HasPrincipalKey(p => p.UserName)
                    .HasForeignKey(d => d.UserName)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("room_pin_ibfk_2");
            });

            modelBuilder.Entity<RoomRecent>(entity =>
            {
                entity.ToTable("room_recent");

                entity.HasCharSet("utf8mb4")
                    .UseCollation("utf8mb4_unicode_ci");

                entity.HasIndex(e => e.RoomUuid, "room_uuid");

                entity.HasIndex(e => e.UserName, "user_name");

                entity.Property(e => e.Id)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("id");

                entity.Property(e => e.Count)
                    .HasColumnType("int(11)")
                    .HasColumnName("count");

                entity.Property(e => e.RoomUuid)
                    .HasMaxLength(36)
                    .HasColumnName("room_uuid")
                    .IsFixedLength()
                    .UseCollation("utf8mb4_unicode_520_ci");

                entity.Property(e => e.TimeCreated)
                    .HasColumnType("timestamp")
                    .HasColumnName("time_created")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.TimeUpdated)
                    .HasColumnType("timestamp")
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnName("time_updated");

                entity.Property(e => e.UserName)
                    .HasMaxLength(50)
                    .HasColumnName("user_name")
                    .UseCollation("utf8mb4_unicode_520_ci");

                entity.HasOne(d => d.RoomUu)
                    .WithMany(p => p.RoomRecent)
                    .HasPrincipalKey(p => p.Uuid)
                    .HasForeignKey(d => d.RoomUuid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("room_recent_ibfk_1");

                entity.HasOne(d => d.UserNameNavigation)
                    .WithMany(p => p.RoomRecent)
                    .HasPrincipalKey(p => p.UserName)
                    .HasForeignKey(d => d.UserName)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("room_recent_ibfk_2");
            });

            modelBuilder.Entity<Rooms>(entity =>
            {
                entity.ToTable("rooms");

                entity.HasCharSet("utf8mb4")
                    .UseCollation("utf8mb4_unicode_520_ci");

                entity.HasIndex(e => e.Creater, "creater");

                entity.HasIndex(e => e.LastUpdated, "last_update_idx");

                entity.HasIndex(e => e.LastMessageUuid, "rooms_ibfk_1");

                entity.HasIndex(e => e.Uuid, "uuid")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("id");

                entity.Property(e => e.Avatar)
                    .HasMaxLength(255)
                    .HasColumnName("avatar")
                    .HasComment("Với type = 2");

                entity.Property(e => e.Creater)
                    .HasMaxLength(50)
                    .HasColumnName("creater");

                entity.Property(e => e.IsAllow)
                    .HasColumnType("bit(1)")
                    .HasColumnName("is_allow")
                    .HasDefaultValueSql("b'0'")
                    .HasComment("Được phép chat với type = 1");

                entity.Property(e => e.LastMessageUuid)
                    .HasMaxLength(36)
                    .HasColumnName("last_message_uuid")
                    .IsFixedLength();

                entity.Property(e => e.LastUpdated)
                    .HasColumnType("timestamp")
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnName("last_updated")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.RoomName)
                    .HasMaxLength(255)
                    .HasColumnName("room_name");

                entity.Property(e => e.Status)
                    .HasColumnType("tinyint(4)")
                    .HasColumnName("status")
                    .HasComment("1: Normal - 2: Pin - 3: Delete Only me - 4: Delete All - 5: Revoke");

                entity.Property(e => e.TimeCreated)
                    .HasColumnType("timestamp")
                    .HasColumnName("time_created")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.Type)
                    .HasColumnType("tinyint(4)")
                    .HasColumnName("type")
                    .HasComment("1: PP - 2: Group");

                entity.Property(e => e.Uuid)
                    .HasMaxLength(36)
                    .HasColumnName("uuid")
                    .HasDefaultValueSql("uuid()")
                    .IsFixedLength();

                entity.HasOne(d => d.CreaterNavigation)
                    .WithMany(p => p.Rooms)
                    .HasPrincipalKey(p => p.UserName)
                    .HasForeignKey(d => d.Creater)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("rooms_ibfk_2");

                entity.HasOne(d => d.LastMessageUu)
                    .WithMany(p => p.Rooms)
                    .HasPrincipalKey(p => p.Uuid)
                    .HasForeignKey(d => d.LastMessageUuid)
                    .HasConstraintName("rooms_ibfk_1");
            });

            modelBuilder.Entity<Session>(entity =>
            {
                entity.ToTable("session");

                entity.HasCharSet("utf8mb4")
                    .UseCollation("utf8mb4_unicode_520_ci");

                entity.HasIndex(e => e.IsOnline, "is_online");

                entity.HasIndex(e => e.UserName, "user_name");

                entity.HasIndex(e => e.Uuid, "uuid")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("id");

                entity.Property(e => e.AccessToken)
                    .HasMaxLength(1000)
                    .HasColumnName("access_token")
                    .HasDefaultValueSql("''")
                    .UseCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.DeviceId)
                    .HasMaxLength(255)
                    .HasColumnName("device_id");

                entity.Property(e => e.FcmToken)
                    .HasMaxLength(255)
                    .HasColumnName("fcm_token")
                    .UseCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.Ip)
                    .HasMaxLength(255)
                    .HasColumnName("ip");

                entity.Property(e => e.IsOnline)
                    .HasColumnType("bit(1)")
                    .HasColumnName("is_online")
                    .HasDefaultValueSql("b'0'");

                entity.Property(e => e.LoginTime)
                    .HasColumnType("timestamp")
                    .HasColumnName("login_time")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.LogoutTime)
                    .HasColumnType("timestamp")
                    .HasColumnName("logout_time");

                entity.Property(e => e.RefreshToken)
                    .HasMaxLength(255)
                    .HasColumnName("refresh_token")
                    .HasDefaultValueSql("''")
                    .UseCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.Status)
                    .HasColumnType("tinyint(4)")
                    .HasColumnName("status")
                    .HasComment("0: Logging - 1: LogOut");

                entity.Property(e => e.TimeConnectSocket)
                    .HasColumnType("timestamp")
                    .HasColumnName("time_connect_socket");

                entity.Property(e => e.TimeDisconnectSocket)
                    .HasColumnType("timestamp")
                    .HasColumnName("time_disconnect_socket");

                entity.Property(e => e.TimeExpired)
                    .HasColumnType("timestamp")
                    .HasColumnName("time_expired")
                    .HasDefaultValueSql("'0000-00-00 00:00:00'");

                entity.Property(e => e.TimeExpiredRefresh)
                    .HasColumnType("timestamp")
                    .HasColumnName("time_expired_refresh")
                    .HasDefaultValueSql("'0000-00-00 00:00:00'");

                entity.Property(e => e.UserName)
                    .HasMaxLength(50)
                    .HasColumnName("user_name");

                entity.Property(e => e.Uuid)
                    .HasMaxLength(36)
                    .HasColumnName("uuid")
                    .IsFixedLength();

                entity.HasOne(d => d.UserNameNavigation)
                    .WithMany(p => p.Session)
                    .HasPrincipalKey(p => p.UserName)
                    .HasForeignKey(d => d.UserName)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("session_ibfk_1");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
