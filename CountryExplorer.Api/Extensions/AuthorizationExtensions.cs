namespace Project_3.Extensions;

public static class AuthorizationExtensions
{
    public static void ConfigureAuthorizationPolicies(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AuthenticatedUser", policy =>
              policy.RequireAuthenticatedUser());

            options.AddPolicy("AdminOnly", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireRole("Admin"));

            options.AddPolicy("UserOrAdmin", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireRole("User", "Admin"));

            options.AddPolicy("User", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireRole("User"));

            options.AddPolicy("AdminWithValidation", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireRole("Admin")
                      .RequireClaim("email"));
        });
    }
}
