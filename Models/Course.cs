namespace AlgoFlow.Models;

public sealed class Course
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public required string Slug { get; set; }

    public required string Summary { get; set; }

    public required string Description { get; set; }

    public CourseLevel Level { get; set; }

    public bool IsPremium { get; set; }

    public bool IsPublished { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<CourseModule> Modules { get; set; } = new List<CourseModule>();
}

public enum CourseLevel
{
    Beginner,
    Intermediate,
    Advanced
}
