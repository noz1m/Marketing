using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Seeds;

public static class DefaultRoles
{
    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var roleName in new[] { "Admin", "Manager", "User" })
        {
            if (await roleManager.RoleExistsAsync(roleName))
                continue;

            var result = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"Could not create role {roleName}: " +
                    string.Join("; ", result.Errors.Select(error => error.Description)));
        }
    }
}
