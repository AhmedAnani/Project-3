namespace Project_3.Extensions;

public static class AuthorizationExtensions
{
    public static void ConfigureAuthorizationPolicies(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireRole("Admin"));

            options.AddPolicy("User", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireRole("User"));

        });
    }
}
