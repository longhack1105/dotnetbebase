using TWChatAppApiMaster.Databases.ChatApp;
using Microsoft.EntityFrameworkCore;
using NLog.Extensions.Logging;

namespace ChatApp.Extensions
{
    public static class ServiceExtension
    {
        private static readonly NLogLoggerFactory LogLoggerFactory = new NLogLoggerFactory();
        private static string ConnectionString { get; set; } = string.Empty;

        public static void ConfigureDbContext(this IServiceCollection services, string connectionString)
        {
            ConnectionString = connectionString;
            services.AddDbContext<DBContext>(o =>
            {
                // Providing details log on DataBase error
                o.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
                o.EnableDetailedErrors();
                o.EnableSensitiveDataLogging();
                o.UseLoggerFactory(LogLoggerFactory);
            });
        }
        public static DBContext GetDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DBContext>();
            optionsBuilder.UseMySql(ConnectionString, ServerVersion.AutoDetect(ConnectionString));
            optionsBuilder.EnableDetailedErrors();
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.UseLoggerFactory(LogLoggerFactory);

            var context = new DBContext(optionsBuilder.Options);
            context.Database.OpenConnection();

            return context;
        }
    }
}