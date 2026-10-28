namespace AlgoFlow.Models.ViewModels;

public sealed class CourseCatalogItemViewModel
{
    public int Id { get; init; }

    public required string Title { get; init; }

    public required string Slug { get; init; }

    public required string Summary { get; init; }

    public CourseLevel Level { get; init; }

    public bool IsPremium { get; init; }

    public int ModuleCount { get; init; }

    public int LessonCount { get; init; }
}
