using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace TWChatAppApiMaster.Databases.ChatApp
{
    public partial class DBContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {       
            // Tự động where IsEnable = 1
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var isEnableProperty = entityType.FindProperty("IsEnable");

                if (isEnableProperty != null && isEnableProperty.ClrType == typeof(ulong))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");

                    var filter = Expression.Equal(
                        Expression.Call(
                            typeof(EF),
                            nameof(EF.Property),
                            new Type[] { typeof(ulong) },
                            parameter,
                            Expression.Constant("IsEnable")
                        ),
                        Expression.Constant(1UL)
                    );

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(Expression.Lambda(filter, parameter));
                }
            }
        }
    }
}
