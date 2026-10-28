using AlgoFlow.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AlgoFlow.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Course> Courses => Set<Course>();

    public DbSet<CourseModule> CourseModules => Set<CourseModule>();

    public DbSet<Lesson> Lessons => Set<Lesson>();

    public DbSet<CourseEnrollment> CourseEnrollments => Set<CourseEnrollment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Course>(entity =>
        {
            entity.HasKey(course => course.Id);
            entity.Property(course => course.Title).HasMaxLength(160).IsRequired();
            entity.Property(course => course.Slug).HasMaxLength(180).IsRequired();
            entity.Property(course => course.Summary).HasMaxLength(500).IsRequired();
            entity.Property(course => course.Description).HasMaxLength(5000).IsRequired();
            entity.Property(course => course.Level).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.HasIndex(course => course.Slug).IsUnique();
            entity.HasIndex(course => new { course.IsPublished, course.Level });
            entity.HasMany(course => course.Modules)
                .WithOne(module => module.Course)
                .HasForeignKey(module => module.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CourseModule>(entity =>
        {
            entity.Property(module => module.Title).HasMaxLength(160).IsRequired();
            entity.HasIndex(module => new { module.CourseId, module.SortOrder }).IsUnique();
            entity.HasMany(module => module.Lessons)
                .WithOne(lesson => lesson.Module)
                .HasForeignKey(lesson => lesson.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Lesson>(entity =>
        {
            entity.Property(lesson => lesson.Title).HasMaxLength(160).IsRequired();
            entity.Property(lesson => lesson.Slug).HasMaxLength(180).IsRequired();
            entity.Property(lesson => lesson.Summary).HasMaxLength(500).IsRequired();
            entity.Property(lesson => lesson.Content).HasMaxLength(10000).IsRequired();
            entity.Property(lesson => lesson.YouTubeVideoId).HasMaxLength(11);
            entity.HasIndex(lesson => new { lesson.ModuleId, lesson.Slug }).IsUnique();
            entity.HasIndex(lesson => new { lesson.ModuleId, lesson.SortOrder }).IsUnique();
        });

        builder.Entity<CourseEnrollment>(entity =>
        {
            entity.Property(enrollment => enrollment.UserId).HasMaxLength(450).IsRequired();
            entity.HasIndex(enrollment => new { enrollment.UserId, enrollment.CourseId }).IsUnique();
            entity.HasIndex(enrollment => enrollment.EnrolledAtUtc);
            entity.HasOne(enrollment => enrollment.User)
                .WithMany()
                .HasForeignKey(enrollment => enrollment.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(enrollment => enrollment.Course)
                .WithMany()
                .HasForeignKey(enrollment => enrollment.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.DisplayName).HasMaxLength(100);
            entity.Property(user => user.CreatedAtUtc).IsRequired();
        });

        builder.Entity<Course>().HasData(
            new Course
            {
                Id = 1,
                Title = "Algorithms & Data Structures: Full Course",
                Slug = "algorithms-data-structures",
                Summary = "A beginner-friendly introduction to the ideas behind common data structures and algorithms.",
                Description = "Build intuition for algorithm analysis, core data structures, and the patterns used to solve programming problems. This starter course uses a freeCodeCamp.org video lesson hosted on YouTube.",
                Level = CourseLevel.Beginner,
                IsPremium = false,
                IsPublished = true,
                CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });

        builder.Entity<CourseModule>().HasData(
            new CourseModule
            {
                Id = 1,
                CourseId = 1,
                Title = "Start with the foundations",
                SortOrder = 1
            });

        builder.Entity<Lesson>().HasData(
            new Lesson
            {
                Id = 1,
                ModuleId = 1,
                Title = "Algorithms and data structures",
                Slug = "algorithms-and-data-structures",
                Summary = "A full-course introduction to algorithms and data structures.",
                Content = "Watch the embedded lesson from freeCodeCamp.org. The video is hosted by YouTube; AlgoFlow does not download or rehost it. Use the course notes to pause and write down unfamiliar concepts, then return to them as you study.",
                YouTubeVideoId = "8hly31xKli0",
                SortOrder = 1
            });
    }
}
