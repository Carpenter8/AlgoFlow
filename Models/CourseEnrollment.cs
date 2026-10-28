namespace AlgoFlow.Models;

public sealed class CourseEnrollment
{
    public int Id { get; set; }

    public required string UserId { get; set; }

    public int CourseId { get; set; }

    public DateTime EnrolledAtUtc { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;

    public Course Course { get; set; } = null!;
}
