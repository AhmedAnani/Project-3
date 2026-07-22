using CountryExplorer.Api.Middleware;
using CountryExplorer.Application.Mappings;
using CountryExplorer.Application.Services;
using CountryExplorer.Application.Services.Interfaces;
using CountryExplorer.Domain.Repositories;
using CountryExplorer.Infrastructure.Data;
using CountryExplorer.Infrastructure.Repositories;
using CountryExplorer.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Project_3.Extensions;
using Project_3.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("Jwt");
var googleSettings = builder.Configuration.GetSection("Google");
var appBaseUrl = builder.Configuration["AppBaseUrl"] ?? "https://localhost:7293";

// ============ SERVICES ============
builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(AuthProfile));
builder.Services.AddLogging();

// ============ AUTHENTICATION ============
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.Name = "CountryExplorer.OAuth";
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
})
.AddGoogle(options =>
{
    options.ClientId = googleSettings["ClientId"] ?? "";
    options.ClientSecret = googleSettings["ClientSecret"] ?? "";
    options.CallbackPath = "/signin-google";
    options.SaveTokens = true;

    options.CorrelationCookie.SameSite = SameSiteMode.None;
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;

    options.Scope.Add("email");
    options.Scope.Add("profile");
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
        ClockSkew = TimeSpan.FromSeconds(10)
    };
});

// ============ DATABASE ============
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    ));

// ============ REPOSITORIES & SERVICES ============
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();

// ============ AUTHORIZATION & API ============
builder.ConfigureAuthorizationPolicies();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Country Explorer API",
        Version = "v1",
        Description = "Authentication and user management API"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ============ CORS ============
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:5048","https://localhost:7293" };

        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// ============ BUILD APP ============
var app = builder.Build();

// ============ MIDDLEWARE PIPELINE ============

app.UseGlobalExceptionHandler();
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthentication();  
app.UseAuthorization();

app.UseRateLimiting();    


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

app.MapControllers();
app.Run();