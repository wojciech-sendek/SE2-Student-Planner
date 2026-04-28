using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudentPlanner.Api.Entities;

namespace StudentPlanner.Api.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public DbSet<Faculty> Faculties => Set<Faculty>();
        public DbSet<PersonalEvent> PersonalEvents => Set<PersonalEvent>();
        public DbSet<UsosEvent> UsosEvents => Set<UsosEvent>();
        public DbSet<UsosToken> UsosTokens => Set<UsosToken>();

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Faculty>(entity =>
            {
                entity.HasKey(f => f.Id);

                entity.Property(f => f.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(f => f.DisplayName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasIndex(f => f.Name)
                    .IsUnique();
            });

            builder.Entity<ApplicationUser>()
                .HasMany(u => u.Faculties)
                .WithMany(f => f.Users);

            builder.Entity<PersonalEvent>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Location)
                    .HasMaxLength(300);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            builder.Entity<UsosEvent>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Location)
                    .HasMaxLength(300);

                entity.Property(e => e.CourseId)
                    .HasMaxLength(100);

                entity.Property(e => e.LecturerName)
                    .HasMaxLength(200);

                entity.Property(e => e.Room)
                    .HasMaxLength(100);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<UsosToken>(entity =>
            {
                entity.HasKey(t => t.Id);

                entity.Property(t => t.AccessToken)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(t => t.AccessTokenSecret)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.HasIndex(t => t.UserId)
                    .IsUnique();

                entity.HasOne(t => t.User)
                    .WithMany()
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}