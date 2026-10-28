namespace AlgoFlow.Models;

public sealed class CourseModule
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public required string Title { get; set; }

    public int SortOrder { get; set; }

    public Course Course { get; set; } = null!;

    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
