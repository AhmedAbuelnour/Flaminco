using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;

namespace WebApplication1
{
    public static class OpenIddictSeedData
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            if (await applicationManager.FindByClientIdAsync("nQv6AI6GxQ2Tfyr4bCZ0") == null)
            {
                await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "nQv6AI6GxQ2Tfyr4bCZ0",
                    ClientSecret = "S5cY3Lgby0kQah5QSmBsX2DJmRr7HQn3Wuaqlotw",
                    DisplayName = "Your Client Application",
                    RedirectUris = { new Uri("https://localhost:5001/callback") },

                    Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.Password,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                }
                });
            }

            // Create user with username and password
            var user = await userManager.FindByNameAsync("admin");

            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = "admin",
                    Email = "admin@example.com",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "Admin@123");
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create test user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }

}
