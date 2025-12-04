using BackEnd.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Infrastructure.Context
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<BlacklistToken> BlacklistTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<BlacklistToken>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Jti)
                      .IsRequired();

                entity.Property(e => e.ExpiryTime)
                      .IsRequired();

                // Create index on JTI for fast blacklist check
                entity.HasIndex(e => e.Jti)
                      .IsUnique();
            });
        }
    }
}
