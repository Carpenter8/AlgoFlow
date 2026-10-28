using System.Text.RegularExpressions;
using AlgoFlow.Data;
using AlgoFlow.Models;
using AlgoFlow.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlgoFlow.Controllers;

public sealed class CoursesController(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    ILogger<CoursesController> logger) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CourseLevel? level, string? search, CancellationToken cancellationToken)
    {
        var coursesQuery = dbContext.Courses
            .AsNoTracking()
            .Where(course => course.IsPublished);

        if (level.HasValue)
        {
            coursesQuery = coursesQuery.Where(course => course.Level == level.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            coursesQuery = coursesQuery.Where(course =>
                course.Title.Contains(term) || course.Summary.Contains(term));
        }

        ViewBag.Level = level;
        ViewBag.Search = search;
        var courses = await coursesQuery
            .Select(course => new CourseCatalogItemViewModel
            {
                Id = course.Id,
                Title = course.Title,
                Slug = course.Slug,
                Summary = course.Summary,
                Level = course.Level,
                IsPremium = course.IsPremium,
                ModuleCount = course.Modules.Count,
                LessonCount = course.Modules.Sum(module => module.Lessons.Count)
            })
            .OrderBy(course => course.Level)
            .ThenBy(course => course.Title)
            .ToListAsync(cancellationToken);

        return View(courses);
    }

    [HttpGet]
    public async Task<IActionResult> Details(string slug, CancellationToken cancellationToken)
    {
        var course = await dbContext.Courses
            .AsNoTracking()
            .Include(item => item.Modules.OrderBy(module => module.SortOrder))
            .ThenInclude(module => module.Lessons.OrderBy(lesson => lesson.SortOrder))
            .FirstOrDefaultAsync(item => item.Slug == slug && item.IsPublished, cancellationToken);

        if (course is null)
        {
            return NotFound();
        }

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = userManager.GetUserId(User);
            ViewBag.IsEnrolled = await dbContext.CourseEnrollments
                .AnyAsync(item => item.UserId == userId && item.CourseId == course.Id, cancellationToken);
        }

        return View(course);
    }

    [Authorize(Roles = "Student,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int courseId, CancellationToken cancellationToken)
    {
        var course = await dbContext.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == courseId && item.IsPublished, cancellationToken);

        if (course is null)
        {
            return NotFound();
        }

        if (course.IsPremium)
        {
            return Forbid();
        }

        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        var exists = await dbContext.CourseEnrollments
            .AnyAsync(item => item.UserId == userId && item.CourseId == course.Id, cancellationToken);

        if (!exists)
        {
            dbContext.CourseEnrollments.Add(new CourseEnrollment
            {
                UserId = userId,
                CourseId = course.Id
            });

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception)
            {
                logger.LogWarning(
                    exception,
                    "Enrollment insert failed for user {UserId} and course {CourseId}.",
                    userId,
                    course.Id);

                dbContext.ChangeTracker.Clear();
                var concurrentEnrollmentExists = await dbContext.CourseEnrollments
                    .AnyAsync(item => item.UserId == userId && item.CourseId == course.Id, cancellationToken);
                if (!concurrentEnrollmentExists)
                {
                    throw;
                }
            }
        }

        return RedirectToAction(nameof(Details), new { slug = course.Slug });
    }

    [Authorize(Roles = "Student,Admin")]
    [HttpGet]
    public async Task<IActionResult> Lesson(
        string courseSlug,
        string lessonSlug,
        CancellationToken cancellationToken)
    {
        var lesson = await dbContext.Lessons
            .AsNoTracking()
            .Include(item => item.Module)
            .ThenInclude(module => module.Course)
            .FirstOrDefaultAsync(
                item => item.Slug == lessonSlug
                    && item.Module.Course.Slug == courseSlug
                    && item.Module.Course.IsPublished,
                cancellationToken);

        if (lesson is null)
        {
            return NotFound();
        }

        var userId = userManager.GetUserId(User);
        var enrolled = userId is not null && await dbContext.CourseEnrollments
            .AnyAsync(
                item => item.UserId == userId && item.CourseId == lesson.Module.CourseId,
                cancellationToken);

        if (!enrolled)
        {
            return RedirectToAction(nameof(Details), new { slug = courseSlug });
        }

        ViewBag.Course = lesson.Module.Course;
        ViewBag.VideoEmbedUrl = IsValidYouTubeVideoId(lesson.YouTubeVideoId)
            ? $"https://www.youtube-nocookie.com/embed/{lesson.YouTubeVideoId}"
            : null;
        return View(lesson);
    }

    private static bool IsValidYouTubeVideoId(string? videoId) =>
        videoId is not null
        && Regex.IsMatch(videoId, "^[A-Za-z0-9_-]{11}$", RegexOptions.CultureInvariant);
}
