using AlgoFlow.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlgoFlow.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class DashboardController(ApplicationDbContext dbContext) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.CourseCount = await dbContext.Courses.CountAsync(cancellationToken);
        ViewBag.StudentCount = await dbContext.Users.CountAsync(cancellationToken);
        ViewBag.EnrollmentCount = await dbContext.CourseEnrollments.CountAsync(cancellationToken);
        return View();
    }
}
