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

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.FirstName)
                    .HasMaxLength(100);

                entity.Property(u => u.LastName)
                    .HasMaxLength(100);

                entity.Property(u => u.UsosRefreshTokenProtected)
                    .HasMaxLength(4000);

                entity.HasMany(u => u.Faculties)
                    .WithMany(f => f.Users);

                entity.HasMany(u => u.PersonalEvents)
                    .WithOne(e => e.User)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.UsosEvents)
                    .WithOne(e => e.User)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<PersonalEvent>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Location)
                    .HasMaxLength(300);
            });

            builder.Entity<UsosEvent>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.ExternalId)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Location)
                    .HasMaxLength(300);

                entity.Property(e => e.Room)
                    .HasMaxLength(100);

                entity.Property(e => e.Teacher)
                    .HasMaxLength(200);

                entity.HasIndex(e => new { e.UserId, e.ExternalId });
            });
        }
    }
}