using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DotnetBeBase.Databases.Quanlytrungtam
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
        public virtual DbSet<Assessment> Assessment { get; set; } = null!;
        public virtual DbSet<Class> Class { get; set; } = null!;
        public virtual DbSet<ClassStudent> ClassStudent { get; set; } = null!;
        public virtual DbSet<ClassTeacher> ClassTeacher { get; set; } = null!;
        public virtual DbSet<Equipment> Equipment { get; set; } = null!;
        public virtual DbSet<EquipmentType> EquipmentType { get; set; } = null!;
        public virtual DbSet<Payment> Payment { get; set; } = null!;
        public virtual DbSet<PaymentStatus> PaymentStatus { get; set; } = null!;
        public virtual DbSet<RollCall> RollCall { get; set; } = null!;
        public virtual DbSet<Room> Room { get; set; } = null!;
        public virtual DbSet<RoomStatus> RoomStatus { get; set; } = null!;
        public virtual DbSet<Schedule> Schedule { get; set; } = null!;
        public virtual DbSet<ScheduleClass> ScheduleClass { get; set; } = null!;
        public virtual DbSet<ScheduleScheduleSheetRoom> ScheduleScheduleSheetRoom { get; set; } = null!;
        public virtual DbSet<ScheduleSheet> ScheduleSheet { get; set; } = null!;
        public virtual DbSet<Session> Session { get; set; } = null!;
        public virtual DbSet<Student> Student { get; set; } = null!;
        public virtual DbSet<Teacher> Teacher { get; set; } = null!;
        public virtual DbSet<Test> Test { get; set; } = null!;
        public virtual DbSet<TestClass> TestClass { get; set; } = null!;
        public virtual DbSet<TestStudent> TestStudent { get; set; } = null!;
        public virtual DbSet<TestType> TestType { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("utf8mb4_uca1400_ai_ci")
                .HasCharSet("utf8mb4");

            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("account");

                entity.HasIndex(e => e.Username, "username")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");

                entity.Property(e => e.Avatar)
                    .HasMaxLength(255)
                    .HasColumnName("avatar");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.Email)
                    .HasMaxLength(255)
                    .HasColumnName("email")
                    .UseCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.FullName)
                    .HasMaxLength(255)
                    .HasColumnName("full_name")
                    .UseCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.Password)
                    .HasMaxLength(255)
                    .HasColumnName("password")
                    .UseCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(20)
                    .HasColumnName("phone_number")
                    .UseCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.RegisterType)
                    .HasColumnType("tinyint(4)")
                    .HasColumnName("register_type")
                    .HasComment("0:email - 1:phone");

                entity.Property(e => e.Role)
                    .HasColumnType("tinyint(4)")
                    .HasColumnName("role")
                    .HasComment("0: admin - 1: giáo viên - 2: học sinh");

                entity.Property(e => e.RoleUuid)
                    .HasMaxLength(36)
                    .HasColumnName("role_uuid")
                    .IsFixedLength();

                entity.Property(e => e.State)
                    .HasColumnType("tinyint(4)")
                    .HasColumnName("state")
                    .HasComment("0: Pending - 1: Active - 2:Deactive");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("updated_at");

                entity.Property(e => e.Username)
                    .HasColumnName("username")
                    .UseCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.Uuid)
                    .HasMaxLength(36)
                    .HasColumnName("uuid")
                    .IsFixedLength()
                    .UseCollation("utf8mb4_unicode_ci");
            });

            modelBuilder.Entity<Assessment>(entity =>
            {
                entity.ToTable("assessment");

                entity.UseCollation("utf8mb4_unicode_ci");

                entity.HasIndex(e => e.ClassUuid, "class_uuid");

                entity.HasIndex(e => e.StudentUuid, "student_uuid");

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");

                entity.Property(e => e.ClassUuid)
                    .HasMaxLength(36)
                    .HasColumnName("class_uuid")
                    .IsFixedLength();

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.Note)
                    .HasColumnType("text")
                    .HasColumnName("note");

                entity.Property(e => e.RoomUuid)
                    .HasMaxLength(36)
                    .HasColumnName("room_uuid")
                    .IsFixedLength();

                entity.Property(e => e.StudentUuid)
                    .HasMaxLength(36)
                    .HasColumnName("student_uuid")
                    .IsFixedLength();

                entity.Property(e => e.TimeSheet)
                    .HasColumnType("timestamp")
                    .HasColumnName("time_sheet")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.Type)
                    .HasColumnType("tinyint(4)")
                    .HasColumnName("type")
                    .HasComment("0: đánh giá chung - 1: khen - 2: chê");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("updated_at");

                entity.Property(e => e.Uuid)
                    .HasMaxLength(36)
                    .HasColumnName("uuid")
                    .IsFixedLength();

                entity.HasOne(d => d.ClassUu)
                    .WithMany(p => p.Assessment)
                    .HasPrincipalKey(p => p.Uuid)
                    .HasForeignKey(d => d.ClassUuid)
                    .HasConstraintName("assessment_ibfk_2");

                entity.HasOne(d => d.StudentUu)
                    .WithMany(p => p.Assessment)
                    .HasPrincipalKey(p => p.Uuid)
                    .HasForeignKey(d => d.StudentUuid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("assessment_ibfk_1");
            });

            modelBuilder.Entity<Class>(entity =>
            {
                entity.ToTable("class");

                entity.UseCollation("utf8mb4_unicode_ci");

                entity.HasIndex(e => e.Uuid, "uuid")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.Des)
                    .HasMaxLength(255)
                    .HasColumnName("des");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.Price)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("price");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("updated_at");

                entity.Property(e => e.Uuid)
                    .HasMaxLength(36)
                    .HasColumnName("uuid")
                    .IsFixedLength();
            });

            modelBuilder.Entity<ClassStudent>(entity =>
            {
                entity.ToTable("class_student");

                entity.UseCollation("utf8mb4_unicode_ci");

                entity.HasIndex(e => e.ClassUuid, "class_uuid");

                entity.HasIndex(e => e.StudentUuid, "student_uuid");

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");

                entity.Property(e => e.ClassUuid)
                    .HasMaxLength(36)
                    .HasColumnName("class_uuid")
                    .IsFixedLength();

                entity.Property(e => e.StudentUuid)
                    .HasMaxLength(36)
                    .HasColumnName("student_uuid")
                    .IsFixedLength();

                entity.HasOne(d => d.ClassUu)
                    .WithMany(p => p.ClassStudent)
                    .HasPrincipalKey(p => p.Uuid)
                    .HasForeignKey(d => d.ClassUuid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("class_student_ibfk_1");

                entity.HasOne(d => d.StudentUu)
                    .WithMany(p => p.ClassStudent)
                    .HasPrincipalKey(p => p.Uuid)
                    .HasForeignKey(d => d.StudentUuid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("class_student_ibfk_2");
            });

            modelBuilder.Entity<ClassTeacher>(entity =>
            {
                entity.ToTable("class_teacher");

                entity.UseCollation("utf8mb4_unicode_ci");

                entity.HasIndex(e => e.ClassUuid, "class_uuid");

                entity.HasIndex(e => e.TeacherUuid, "teacher_uuid");

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");

                entity.Property(e => e.ClassUuid)
                    .HasMaxLength(36)
                    .HasColumnName("class_uuid")
                    .IsFixedLength();

                entity.Property(e => e.TeacherUuid)
                    .HasMaxLength(36)
                    .HasColumnName("teacher_uuid")
                    .IsFixedLength();

                entity.HasOne(d => d.ClassUu)
                    .WithMany(p => p.ClassTeacher)
                    .HasPrincipalKey(p => p.Uuid)
                    .HasForeignKey(d => d.ClassUuid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("class_teacher_ibfk_1");

                entity.HasOne(d => d.TeacherUu)
                    .WithMany(p => p.ClassTeacher)
                    .HasPrincipalKey(p => p.Uuid)
                    .HasForeignKey(d => d.TeacherUuid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("class_teacher_ibfk_2");
            });

            modelBuilder.Entity<Equipment>(entity =>
            {
                entity.ToTable("equipment");

                entity.UseCollation("utf8mb4_unicode_ci");

                entity.HasIndex(e => e.RoomUuid, "room_uuid");

                entity.HasIndex(e => e.Type, "type");

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.Des)
                    .HasMaxLength(255)
                    .HasColumnName("des");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.Price)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("price");

                entity.Property(e => e.RoomUuid)
                    .HasMaxLength(36)
                    .HasColumnName("room_uuid")
                    .IsFixedLength();

                entity.Property(e => e.Type)
                    .HasColumnType("tinyint(4)")
                    .HasColumnName("type");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("updated_at");

                entity.Property(e => e.Uuid)
                    .HasMaxLength(36)
                    .HasColumnName("uuid")
                    .IsFixedLength();

                entity.HasOne(d => d.RoomUu)
                    .WithMany(p => p.Equipment)
                    .HasPrincipalKey(p => p.Uuid)
                    .HasForeignKey(d => d.RoomUuid)
                    .HasConstraintName("equipment_ibfk_1");

                entity.HasOne(d => d.TypeNavigation)
                    .WithMany(p => p.Equipment)
                    .HasForeignKey(d => d.Type)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("equipment_ibfk_2");
            });

            modelBuilder.Entity<EquipmentType>(entity =>
            {
                entity.ToTable("equipment_type");

                entity.UseCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.Id)
                    .HasColumnType("tinyint(4)")
                    .HasColumnName("id");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("payment");

                entity.UseCollation("utf8mb4_unicode_ci");

                entity.HasIndex(e => e.Status, "status");

                entity.HasIndex(e => e.StudentUuid, "student_uuid");

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");

                entity.Property(e => e.Amount)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("amount");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.Status)
                    .HasColumnType("tinyint(4)")
                    .HasColumnName("status");

                entity.Property(e => e.StudentUuid)
                    .HasMaxLength(36)
                    .HasColumnName("student_uuid")
                    .IsFixedLength();

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("updated_at");

                entity.Property(e => e.Uuid)
                    .HasMaxLength(36)
                    .HasColumnName("uuid")
                    .IsFixedLength();

                entity.HasOne(d => d.StatusNavigation)
                    .WithMany(p => p.Payment)
                    .HasForeignKey(d => d.Status)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("payment_ibfk_2");

                entity.HasOne(d => d.StudentUu)
                    .WithMany(p => p.Payment)
                    .HasPrincipalKey(p => p.Uuid)
                    .HasForeignKey(d => d.StudentUuid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("payment_ibfk_1");
            });

            modelBuilder.Entity<PaymentStatus>(entity =>
            {
                entity.ToTable("payment_status");

                entity.UseCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.Id)
                    .HasColumnType("tinyint(4)")
                    .HasColumnName("id");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<RollCall>(entity =>
            {
                entity.ToTable("roll_call");

                entity.UseCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");

                entity.Property(e => e.ClassUuid)
                    .HasMaxLength(36)
                    .HasColumnName("class_uuid")
                    .IsFixedLength();

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.RoomUuid)
                    .HasMaxLength(36)
                    .HasColumnName("room_uuid")
                    .IsFixedLength();

                entity.Property(e => e.StudentUuid)
                    .HasMaxLength(36)
                    .HasColumnName("student_uuid")
                    .IsFixedLength();

                entity.Property(e => e.TimeSheet)
                    .HasColumnType("timestamp")
                    .HasColumnName("time_sheet")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.ToTable("room");

                entity.UseCollation("utf8mb4_unicode_ci");

                entity.HasIndex(e => e.Status, "status");

                entity.HasIndex(e => e.Uuid, "uuid")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.Status)
                    .HasColumnType("tinyint(4)")
                    .HasColumnName("status");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("updated_at");

                entity.Property(e => e.Uuid)
                    .HasMaxLength(36)
                    .HasColumnName("uuid")
                    .IsFixedLength();

                entity.HasOne(d => d.StatusNavigation)
                    .WithMany(p => p.Room)
                    .HasForeignKey(d => d.Status)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("room_ibfk_1");
            });

            modelBuilder.Entity<RoomStatus>(entity =>
            {
                entity.ToTable("room_status");

                entity.UseCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.Id)
                    .HasColumnType("tinyint(4)")
                    .HasColumnName("id");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.ToTable("schedule");

                entity.UseCollation("utf8mb4_unicode_ci");

                entity.HasIndex(e => e.Uuid, "uuid")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.Status)
                    .HasColumnType("tinyint(4)")
                    .HasColumnName("status")
                    .HasComment("0: Hoạt động, 1: Ngừng hoạt động");

                entity.Property(e => e.TimeEnd)
                    .HasColumnType("datetime")
                    .HasColumnName("time_end");

                entity.Property(e => e.TimeStart)
                    .HasColumnType("datetime")
                    .HasColumnName("time_start");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("updated_at");

                entity.Property(e => e.Uuid)
                    .HasMaxLength(36)
                    .HasColumnName("uuid")
                    .IsFixedLength();
            });

            modelBuilder.Entity<ScheduleClass>(entity =>
            {
                entity.ToTable("schedule_class");

                entity.UseCollation("utf8mb4_unicode_ci");

                entity.HasIndex(e => e.ClassUuid, "class_uuid");

                entity.HasIndex(e => e.ScheduleUuid, "schedule_uuid");

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");

                entity.Property(e => e.ClassUuid)
                    .HasMaxLength(36)
                    .HasColumnName("class_uuid")
                    .IsFixedLength();

                entity.Property(e => e.ScheduleUuid)
                    .HasMaxLength(36)
                    .HasColumnName("schedule_uuid")
                    .IsFixedLength();

                entity.HasOne(d => d.ClassUu)
                    .WithMany(p => p.ScheduleClass)
                    .HasPrincipalKey(p => p.Uuid)
                    .HasForeignKey(d => d.ClassUuid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("schedule_class_ibfk_2");

                entity.HasOne(d => d.ScheduleUu)
                    .WithMany(p => p.ScheduleClass)
                    .HasPrincipalKey(p => p.Uuid)
                    .HasForeignKey(d => d.ScheduleUuid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("schedule_class_ibfk_1");
            });

            modelBuilder.Entity<ScheduleScheduleSheetRoom>(entity =>
            {
                entity.ToTable("schedule_schedule_sheet_room");

                entity.UseCollation("utf8mb4_unicode_ci");

                entity.HasIndex(e => e.RoomUuid, "room_uuid");

                entity.HasIndex(e => e.ScheduleSheetUuid, "schedule_sheet_uuid");

                entity.HasIndex(e => e.ScheduleUuid, "schedule_uuid");

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");

                entity.Property(e => e.RoomUuid)
                    .HasMaxLength(36)
                    .HasColumnName("room_uuid")
                    .IsFixedLength();

                entity.Property(e => e.ScheduleSheetUuid)
                    .HasMaxLength(36)
                    .HasColumnName("schedule_sheet_uuid")
                    .IsFixedLength();

                entity.Property(e => e.ScheduleUuid)
                    .HasMaxLength(36)
                    .HasColumnName("schedule_uuid")
                    .IsFixedLength();

                entity.HasOne(d => d.RoomUu)
                    .WithMany(p => p.ScheduleScheduleSheetRoom)
                    .HasPrincipalKey(p => p.Uuid)
                    .HasForeignKey(d => d.RoomUuid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("schedule_schedule_sheet_room_ibfk_3");

                entity.HasOne(d => d.ScheduleSheetUu)
                    .WithMany(p => p.ScheduleScheduleSheetRoom)
                    .HasPrincipalKey(p => p.Uuid)
                    .HasForeignKey(d => d.ScheduleSheetUuid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("schedule_schedule_sheet_room_ibfk_2");

                entity.HasOne(d => d.ScheduleUu)
                    .WithMany(p => p.ScheduleScheduleSheetRoom)
                    .HasPrincipalKey(p => p.Uuid)
                    .HasForeignKey(d => d.ScheduleUuid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("schedule_schedule_sheet_room_ibfk_1");
            });

            modelBuilder.Entity<ScheduleSheet>(entity =>
            {
                entity.ToTable("schedule_sheet");

                entity.UseCollation("utf8mb4_unicode_ci");

                entity.HasIndex(e => e.Uuid, "uuid")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");

                entity.Property(e => e.DayInWeek)
                    .HasColumnType("tinyint(4)")
                    .HasColumnName("day_in_week")
                    .HasComment("2 -> 8");

                entity.Property(e => e.TimeEnd)
                    .HasMaxLength(5)
                    .HasColumnName("time_end")
                    .HasDefaultValueSql("current_timestamp(5)")
                    .HasComment("format: HH:mm");

                entity.Property(e => e.TimeStart)
                    .HasMaxLength(5)
                    .HasColumnName("time_start")
                    .HasDefaultValueSql("current_timestamp(5)")
                    .HasComment("format: HH:mm");

                entity.Property(e => e.Uuid)
                    .HasMaxLength(36)
                    .HasColumnName("uuid")
                    .IsFixedLength();
            });

            modelBuilder.Entity<Session>(entity =>
            {
                entity.ToTable("session");

                entity.HasIndex(e => e.Username, "username");

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");

                entity.Property(e => e.AccountUuid)
                    .HasMaxLength(36)
                    .HasColumnName("account_uuid")
                    .IsFixedLength()
                    .UseCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.RefreshToken)
                    .HasColumnType("text")
                    .HasColumnName("refresh_token")
                    .UseCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.State)
                    .HasColumnType("tinyint(4)")
                    .HasColumnName("state")
                    .HasComment("1 - active; 2 - destroy");

                entity.Property(e => e.TimeExpired)
                    .HasColumnType("datetime")
                    .HasColumnName("time_expired");

                entity.Property(e => e.TimeRefreshExpired)
                    .HasColumnType("datetime")
                    .HasColumnName("time_refresh_expired");

                entity.Property(e => e.Token)
                    .HasColumnType("text")
                    .HasColumnName("token");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("updated_at");

                entity.Property(e => e.Username)
                    .HasColumnName("username")
                    .UseCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.Uuid)
                    .HasMaxLength(36)
                    .HasColumnName("uuid")
                    .IsFixedLength()
                    .UseCollation("utf8mb4_unicode_ci");

                entity.HasOne(d => d.UsernameNavigation)
                    .WithMany(p => p.Session)
                    .HasPrincipalKey(p => p.Username)
                    .HasForeignKey(d => d.Username)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("session_ibfk_1");
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.ToTable("student");

                entity.UseCollation("utf8mb4_unicode_ci");

                entity.HasIndex(e => e.Uuid, "uuid")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.Email)
                    .HasMaxLength(255)
                    .HasColumnName("email");

                entity.Property(e => e.FullName)
                    .HasMaxLength(255)
                    .HasColumnName("full_name");

                entity.Property(e => e.Messenge)
                    .HasMaxLength(255)
                    .HasColumnName("messenge");

                entity.Property(e => e.Phone)
                    .HasMaxLength(255)
                    .HasColumnName("phone");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("updated_at");

                entity.Property(e => e.Uuid)
                    .HasMaxLength(36)
                    .HasColumnName("uuid")
                    .IsFixedLength();

                entity.Property(e => e.Zalo)
                    .HasMaxLength(255)
                    .HasColumnName("zalo");
            });

            modelBuilder.Entity<Teacher>(entity =>
            {
                entity.ToTable("teacher");

                entity.UseCollation("utf8mb4_unicode_ci");

                entity.HasIndex(e => e.Uuid, "uuid")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.Email)
                    .HasMaxLength(255)
                    .HasColumnName("email");

                entity.Property(e => e.FullName)
                    .HasMaxLength(255)
                    .HasColumnName("full_name");

                entity.Property(e => e.Phone)
                    .HasMaxLength(255)
                    .HasColumnName("phone");

                entity.Property(e => e.Salary)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("salary");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("updated_at");

                entity.Property(e => e.Uuid)
                    .HasMaxLength(36)
                    .HasColumnName("uuid")
                    .IsFixedLength();
            });

            modelBuilder.Entity<Test>(entity =>
            {
                entity.ToTable("test");

                entity.UseCollation("utf8mb4_unicode_ci");

                entity.HasIndex(e => e.Type, "type");

                entity.HasIndex(e => e.Uuid, "uuid")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.Des)
                    .HasMaxLength(255)
                    .HasColumnName("des");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.Type)
                    .HasColumnType("tinyint(4)")
                    .HasColumnName("type");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("updated_at");

                entity.Property(e => e.Uuid)
                    .HasMaxLength(36)
                    .HasColumnName("uuid")
                    .IsFixedLength();

                entity.HasOne(d => d.TypeNavigation)
                    .WithMany(p => p.Test)
                    .HasForeignKey(d => d.Type)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("test_ibfk_1");
            });

            modelBuilder.Entity<TestClass>(entity =>
            {
                entity.ToTable("test_class");

                entity.UseCollation("utf8mb4_unicode_ci");

                entity.HasIndex(e => e.ClassUuid, "class_uuid");

                entity.HasIndex(e => e.ScheduleScheduleSheetRoomId, "schedule_schedule_sheet_room_id");

                entity.HasIndex(e => e.TestUuid, "test_uuid");

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");

                entity.Property(e => e.ClassUuid)
                    .HasMaxLength(36)
                    .HasColumnName("class_uuid")
                    .IsFixedLength();

                entity.Property(e => e.ScheduleScheduleSheetRoomId)
                    .HasColumnType("int(11)")
                    .HasColumnName("schedule_schedule_sheet_room_id");

                entity.Property(e => e.TestUuid)
                    .HasMaxLength(36)
                    .HasColumnName("test_uuid")
                    .IsFixedLength();

                entity.HasOne(d => d.ClassUu)
                    .WithMany(p => p.TestClass)
                    .HasPrincipalKey(p => p.Uuid)
                    .HasForeignKey(d => d.ClassUuid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("test_class_ibfk_2");

                entity.HasOne(d => d.ScheduleScheduleSheetRoom)
                    .WithMany(p => p.TestClass)
                    .HasForeignKey(d => d.ScheduleScheduleSheetRoomId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("test_class_ibfk_3");

                entity.HasOne(d => d.TestUu)
                    .WithMany(p => p.TestClass)
                    .HasPrincipalKey(p => p.Uuid)
                    .HasForeignKey(d => d.TestUuid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("test_class_ibfk_1");
            });

            modelBuilder.Entity<TestStudent>(entity =>
            {
                entity.ToTable("test_student");

                entity.UseCollation("utf8mb4_unicode_ci");

                entity.HasIndex(e => e.StudentUuid, "student_uuid");

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .ValueGeneratedOnAddOrUpdate()
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.Grade)
                    .HasPrecision(10, 2)
                    .HasColumnName("grade");

                entity.Property(e => e.Note)
                    .HasMaxLength(255)
                    .HasColumnName("note");

                entity.Property(e => e.StudentUuid)
                    .HasMaxLength(36)
                    .HasColumnName("student_uuid")
                    .IsFixedLength();

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("updated_at");

                entity.HasOne(d => d.StudentUu)
                    .WithMany(p => p.TestStudent)
                    .HasPrincipalKey(p => p.Uuid)
                    .HasForeignKey(d => d.StudentUuid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("test_student_ibfk_1");
            });

            modelBuilder.Entity<TestType>(entity =>
            {
                entity.ToTable("test_type");

                entity.UseCollation("utf8mb4_unicode_ci");

                entity.Property(e => e.Id)
                    .HasColumnType("tinyint(4)")
                    .HasColumnName("id");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
