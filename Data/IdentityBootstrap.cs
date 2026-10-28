using AlgoFlow.Models;
using Microsoft.AspNetCore.Identity;

namespace AlgoFlow.Data;

public static class IdentityBootstrap
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("AlgoFlow.IdentityBootstrap");

        var email = configuration["ALGOFLOW_BOOTSTRAP_ADMIN_EMAIL"];
        var password = configuration["ALGOFLOW_BOOTSTRAP_ADMIN_PASSWORD"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Set both ALGOFLOW_BOOTSTRAP_ADMIN_EMAIL and ALGOFLOW_BOOTSTRAP_ADMIN_PASSWORD before running the --bootstrap-admin command.");
        }

        foreach (var role in new[] { "Student", "Admin" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Could not create the required {role} role: {string.Join("; ", result.Errors.Select(error => error.Description))}");
                }
            }
        }

        var existingUser = await userManager.FindByEmailAsync(email.Trim());
        if (existingUser is not null)
        {
            if (!await userManager.IsInRoleAsync(existingUser, "Admin"))
            {
                throw new InvalidOperationException(
                    "The bootstrap administrator email already belongs to a non-admin account. No role was changed.");
            }

            logger.LogInformation("The configured bootstrap administrator already exists.");
            return;
        }

        var admin = new ApplicationUser
        {
            UserName = email.Trim(),
            Email = email.Trim(),
            EmailConfirmed = true,
            DisplayName = "AlgoFlow Administrator",
            CreatedAtUtc = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(admin, password);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Could not create the bootstrap administrator: {string.Join("; ", createResult.Errors.Select(error => error.Description))}");
        }

        var roleResult = await userManager.AddToRoleAsync(admin, "Admin");
        if (!roleResult.Succeeded)
        {
            var rollback = await userManager.DeleteAsync(admin);
            if (!rollback.Succeeded)
            {
                logger.LogError("Could not remove partially created bootstrap account {UserId}.", admin.Id);
            }

            throw new InvalidOperationException(
                $"Could not assign the Admin role to the bootstrap account: {string.Join("; ", roleResult.Errors.Select(error => error.Description))}");
        }

        logger.LogInformation("Created the configured bootstrap administrator account.");
    }
}
