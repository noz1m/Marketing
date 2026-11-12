using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Seeds;

public static class DefaultUsers
{
    public static async Task SeedAdminAsync(
        UserManager<IdentityUser> userManager,
        IConfiguration configuration)
    {
        var email = configuration["SeedAdmin:Email"];
        var password = configuration["SeedAdmin:Password"];

        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(password))
            return;
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("SeedAdmin:Email and SeedAdmin:Password must both be set.");

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    "Could not seed admin: " +
                    string.Join("; ", result.Errors.Select(error => error.Description)));
        }

        if (!await userManager.IsInRoleAsync(user, "Admin"))
        {
            var result = await userManager.AddToRoleAsync(user, "Admin");
            if (!result.Succeeded)
                throw new InvalidOperationException("Could not assign Admin role.");
        }
    }
}
