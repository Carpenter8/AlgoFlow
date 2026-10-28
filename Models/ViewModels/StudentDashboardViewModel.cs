namespace AlgoFlow.Models.ViewModels;

public sealed class StudentDashboardViewModel
{
    public required string DisplayName { get; init; }

    public required IReadOnlyList<Course> Courses { get; init; }
}
