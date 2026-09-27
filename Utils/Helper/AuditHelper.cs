using Microsoft.EntityFrameworkCore;

namespace BackEnd.Utils.Helper
{
    public interface IAuditableEntity
    {
        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt { get; set; }
    }

    public class AuditHelper
    {
        public static void UpdateAuditFields(DbContext context)
        {
            var entries = context.ChangeTracker.Entries<IAuditableEntity>();

            var now = DateTime.UtcNow;

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = now;
                }
            }
        }
    }
}
