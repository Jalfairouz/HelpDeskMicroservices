using UserManagementService.Models;
using Microsoft.AspNetCore.Identity;

namespace UserManagementService.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync( IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        var userManager = scope.ServiceProvider .GetRequiredService<UserManager<ApplicationUser>>();

        await CreateRolesAsync(roleManager);
        await CreateAdminAsync(userManager, configuration);
    }

    private static async Task CreateRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        string[] roles =
        [
            RoleNames.Employee,
            RoleNames.Technician,
            RoleNames.Admin
        ];

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }
    }

    private static async Task CreateAdminAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        var email = configuration["Admin:Email"];
        var password = configuration["Admin:Password"];

        if (string.IsNullOrWhiteSpace(email) ||  string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException( "Admin settings are missing.");
        }

        var admin = await userManager.FindByEmailAsync(email);

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = "System",
                LastName = "Admin",
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult =await userManager.CreateAsync(admin, password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join( ", ", createResult.Errors.Select( error => error.Description));

                throw new InvalidOperationException(errors);
            }
        }

        if (!await userManager.IsInRoleAsync( admin, RoleNames.Admin))
        {
            await userManager.AddToRoleAsync( admin,  RoleNames.Admin);
        }
    }
}