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
        public DbSet<AcademicEvent> AcademicEvents => Set<AcademicEvent>();
        public DbSet<EventRequest> EventRequests => Set<EventRequest>();

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

            builder.Entity<AcademicEvent>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Location)
                    .HasMaxLength(300);

                entity.HasOne(e => e.Faculty)
                    .WithMany()
                    .HasForeignKey(e => e.FacultyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.FacultyId, e.StartTime });
            });

            builder.Entity<EventRequest>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.Property(r => r.RequestType)
                    .HasConversion<string>()
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(r => r.Status)
                    .HasConversion<string>()
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(r => r.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(r => r.Location)
                    .HasMaxLength(300);

                entity.Property(r => r.ReviewComment)
                    .HasMaxLength(1000);

                entity.HasOne(r => r.Faculty)
                    .WithMany()
                    .HasForeignKey(r => r.FacultyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Manager)
                    .WithMany()
                    .HasForeignKey(r => r.ManagerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Admin)
                    .WithMany()
                    .HasForeignKey(r => r.AdminId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.TargetAcademicEvent)
                    .WithMany(e => e.EventRequests)
                    .HasForeignKey(r => r.TargetAcademicEventId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(r => new { r.ManagerId, r.CreatedAtUtc });
                entity.HasIndex(r => new { r.FacultyId, r.Status });
            });

        }
    }
}