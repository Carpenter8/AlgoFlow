using Microsoft.AspNetCore.Identity;

namespace AlgoFlow.Models;

public sealed class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
