namespace AlgoFlow.Models;

public sealed class Lesson
{
    public int Id { get; set; }

    public int ModuleId { get; set; }

    public required string Title { get; set; }

    public required string Slug { get; set; }

    public required string Summary { get; set; }

    public required string Content { get; set; }

    public string? YouTubeVideoId { get; set; }

    public int SortOrder { get; set; }

    public CourseModule Module { get; set; } = null!;
}
