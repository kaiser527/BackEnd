using BackEnd.Modules.Auth.Entities;
using BackEnd.Modules.History.Entities;
using BackEnd.Modules.QuizApp.Entities;
using BackEnd.Modules.QuizContent.Entities;
using BackEnd.Modules.User.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Modules.Database
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<BlacklistToken> BlacklistTokens => Set<BlacklistToken>();
        public DbSet<Quiz> Quizzes => Set<Quiz>();
        public DbSet<Content> Contents => Set<Content>();
        public DbSet<Question> Questions => Set<Question>();
        public DbSet<Answer> Answers => Set<Answer>();
        public DbSet<QuizHistory> QuizHistories => Set<QuizHistory>();
        public DbSet<UserAnswer> UserAnswers => Set<UserAnswer>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // BlacklistToken
            builder.Entity<BlacklistToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Jti).IsRequired();
                entity.Property(e => e.ExpiryTime).IsRequired();
                entity.HasIndex(e => e.Jti).IsUnique();
            });

            // Quiz
            builder.Entity<Quiz>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.Image).IsRequired();
                entity.Property(e => e.TimeSeconds).IsRequired();
                entity.HasMany(e => e.Contents)
                    .WithOne(e => e.Quiz)
                    .HasForeignKey(e => e.QuizId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Content
            builder.Entity<Content>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Instruction).IsRequired();
                entity.HasMany(e => e.Questions)
                    .WithOne(e => e.Content)
                    .HasForeignKey(e => e.ContentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Question
            builder.Entity<Question>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Text).IsRequired();
                entity.HasMany(e => e.Answers)
                    .WithOne(e => e.Question)
                    .HasForeignKey(e => e.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Answer
            builder.Entity<Answer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Text).IsRequired();
            });

            // QuizHistory
            builder.Entity<QuizHistory>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasOne(x => x.User)
                    .WithMany(x => x.QuizHistories)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.Quiz)
                    .WithMany(x => x.QuizHistories)
                    .HasForeignKey(x => x.QuizId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(x => x.UserAnswers)
                    .WithOne(x => x.QuizHistory)
                    .HasForeignKey(x => x.QuizHistoryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // UserAnswer
            builder.Entity<UserAnswer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.QuizHistory)
                    .WithMany(q => q.UserAnswers)
                    .HasForeignKey(e => e.QuizHistoryId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Question)
                    .WithMany(q => q.UserAnswers)
                    .HasForeignKey(e => e.QuestionId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.SelectedAnswer)
                    .WithMany(a => a.UserAnswers)
                    .HasForeignKey(e => e.SelectedAnswerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}