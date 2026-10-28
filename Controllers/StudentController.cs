using AlgoFlow.Data;
using AlgoFlow.Models;
using AlgoFlow.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlgoFlow.Controllers;

[Authorize(Roles = "Student,Admin")]
public sealed class StudentController(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var courses = await dbContext.CourseEnrollments
            .AsNoTracking()
            .Where(enrollment => enrollment.UserId == userId)
            .Select(enrollment => enrollment.Course)
            .OrderBy(course => course.Title)
            .ToListAsync(cancellationToken);

        return View(new StudentDashboardViewModel
        {
            DisplayName = user.DisplayName ?? user.Email ?? "Student",
            Courses = courses
        });
    }
}
