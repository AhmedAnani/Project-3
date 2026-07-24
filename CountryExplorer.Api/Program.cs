using CountryExplorer.Application.Interfaces.Services;
using CountryExplorer.Application.Interfaces.External;
using CountryExplorer.Application.Services;
using CountryExplorer.Infrastructure.ExternalServices;
using CountryExplorer.Api.Middleware;
using CountryExplorer.Application.Mappings;
using CountryExplorer.Application.Services;
using CountryExplorer.Application.Services.Interfaces;
using CountryExplorer.Domain.Repositories;
using CountryExplorer.Infrastructure.Data;
using CountryExplorer.Infrastructure.Data.Seeding;
using CountryExplorer.Infrastructure.Repositories;
using CountryExplorer.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using CountryExplorer.Application;
using CountryExplorer.Infrastructure;
using CountryExplorer.Api.Middlewares;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Project_3.Extensions;
using Project_3.Middleware;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("Jwt");
var googleSettings = builder.Configuration.GetSection("Google");
var appBaseUrl = builder.Configuration["AppBaseUrl"] ?? "https://localhost:7293";

// ============ SERVICES ============
builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(AuthProfile));
builder.Services.AddLogging();

// AutoMapper — scan both Application (Trip profiles) and Infrastructure (Country profiles) assemblies
builder.Services.AddAutoMapper(
    typeof(CountryExplorer.Application.Mappings.TripMappingProfile).Assembly,
    typeof(CountryExplorer.Infrastructure.Mappings.MappingProfile).Assembly
);

builder.Services.AddMemoryCache();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Application & Infrastructure DI (Trip service, repository, Google Calendar)
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();

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

// ============ DATABASE ============
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    ));

// Esraa's external API clients
builder.Services.AddHttpClient<ICountryApiService, CountryApiService>(client =>
{
    client.BaseAddress = new Uri("https://api.restcountries.com/countries/v5/");
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {builder.Configuration["RestCountries:ApiKey"]}");

});

builder.Services.AddHttpClient<ITouristAttractionService, TouristAttractionService>(client =>
{
    client.BaseAddress = new Uri("https://api.opentripmap.com/0.1/en/places/");
});

builder.Services.AddHttpClient<IExchangeRateService, ExchangeRateService>(client =>
{
    client.BaseAddress = new Uri("https://v6.exchangerate-api.com/v6/");
});

builder.Services.AddScoped<ICountryExplorerService, CountryExplorerService>();

var app = builder.Build();
// ============ REPOSITORIES & SERVICES ============
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
builder.Services.AddScoped<IUserService, UserService>();


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

using (IServiceScope scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme"
    });

    try
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();

        await DbSeed.SeedAsync(dbContext);
    }
    catch (Exception ex)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}
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

app.UseExceptionHandler();
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