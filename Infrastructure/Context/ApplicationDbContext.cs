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

            builder.Entity<ChatRoomUser>()
                .HasKey(x => new { x.ChatRoomId, x.UserId });

            builder.Entity<Message>()
                .HasIndex(m => new { m.ChatRoomId, m.CreatedAt });

            builder.Entity<ChatRoomUser>()
                .HasOne(x => x.ChatRoom)
                .WithMany(r => r.ChatRoomUsers)
                .HasForeignKey(x => x.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ChatRoomUser>()
                .HasOne(x => x.User)
                .WithMany(u => u.ChatRoomUsers)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Message>()
                .HasOne(m => m.User)
                .WithMany(u => u.Messages)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(m => m.ChatRoom)
                .WithMany(r => r.Messages)
                .HasForeignKey(m => m.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<BlacklistToken>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Jti)
                      .IsRequired();

                entity.Property(e => e.ExpiryTime)
                      .IsRequired();

                entity.HasIndex(e => e.Jti)
                      .IsUnique();
            });
        }
    }
}
