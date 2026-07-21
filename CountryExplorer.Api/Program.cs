using CountryExplorer.Infrastructure.Data;
using CountryExplorer.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using CountryExplorer.Application;
using CountryExplorer.Infrastructure;
using CountryExplorer.Api.Middlewares;
using CountryExplorer.Domain.Entities;
using CountryExplorer.Domain.Enums;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();

    try
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();

        var dummyUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var dummyUserExists = await dbContext.Users.AnyAsync(u => u.Id == dummyUserId);

        if (!dummyUserExists)
        {
            dbContext.Users.Add(new User
            {
                Id = dummyUserId,
                FullName = "Test User",
                Email = "test.user@example.com",
                PasswordHash = "dummy-password-hash",
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            });

            await dbContext.SaveChangesAsync();
        }

        await DbSeed.SeedAsync(dbContext);
    }
    catch (Exception ex)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();