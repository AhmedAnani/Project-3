using CountryExplorer.Api.Middlewares;
using CountryExplorer.Infrastructure.Data;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using CountryExplorer.Application;
using CountryExplorer.Infrastructure;

using Microsoft.IdentityModel.Tokens;
using CountryExplorer.Api.Extensions;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("Jwt");
var googleSettings = builder.Configuration.GetSection("Google");
//var appBaseUrl = builder.Configuration["AppBaseUrl"] ?? "https://localhost:7293";

// ============ SERVICES ============
builder.Services.AddControllers();
builder.Services.AddLogging();
builder.Services.AddMemoryCache();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// AutoMapper — scan both Application (Trip profiles) and Infrastructure (Country profiles) assemblies
builder.Services.AddAutoMapper(
    typeof(CountryExplorer.Application.Mappings.TripMappingProfile).Assembly,
    typeof(CountryExplorer.Infrastructure.Mappings.MappingProfile).Assembly
);

// Application & Infrastructure DI (Trip service, repository, Google Calendar)

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

// ============ AUTHENTICATION ============
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.Name = "CountryExplorer.OAuth";
})
.AddGoogle(options =>
{
    options.ClientId = googleSettings["ClientId"] ?? "";
    options.ClientSecret = googleSettings["ClientSecret"] ?? "";
    options.CallbackPath = "/signin-google";
    options.SaveTokens = true;
    options.CorrelationCookie.SameSite = SameSiteMode.Lax;
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Scope.Add("email");
    options.Scope.Add("profile");
    options.Scope.Add("https://www.googleapis.com/auth/calendar");
    options.AccessType = "offline";                 
    options.ClaimActions.MapJsonKey("picture", "picture");
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("Missing JWT key"))
        ),
        RoleClaimType = ClaimTypes.Role,
        ClockSkew = TimeSpan.Zero
    };
});


// DEPENDENCY INJECTION
builder.Services.AddApplicationServices();

builder.Services.AddInfrastructureServices(builder.Configuration);

// ============ AUTHORIZATION & API ============
builder.ConfigureAuthorizationPolicies();

// ============ SWAGGER ============
builder.ConfigureSwagger();

// ============ CORS ============

builder.ConfigureCors();

// ============ BUILD APP ============
var app = builder.Build();

// ============ SWAGGER & DATABASE ============
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Country Explorer API v1");
    });

    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Database migration failed");
        }
    }
}

// ============ MIDDLEWARE PIPELINE ============

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthentication();  
app.UseAuthorization();

app.UseRateLimiting();    

app.MapControllers();
app.Run();