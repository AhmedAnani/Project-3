using CountryExplorer.Domain.Entities;
using CountryExplorer.Domain.Enums;
using CountryExplorer.Infrastructure.Data;

namespace CountryExplorer.Infrastructure.Seeding;

public static class DbSeed
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (context.Users.Any())
            return;

        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Admin",
            Email = "admin@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        };

        var normalUser = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Esraa",
            Email = "esraa@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Esraa@123"),
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        await context.Users.AddRangeAsync(adminUser, normalUser);

        var tripItems = new List<TripBucketItem>
        {
            new()
            {
                UserId = normalUser.Id,
                Title = "Japan Spring Trip",
                CountryCode = "JPN",
                CountryName = "Japan",
                Status = TripStatus.Planned,
                StartDate = DateTime.UtcNow.AddMonths(3),
                EndDate = DateTime.UtcNow.AddMonths(3).AddDays(7),
                Notes = "Cherry blossom season trip",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                UserId = normalUser.Id,
                Title = "Egypt History Trip",
                CountryCode = "EGY",
                CountryName = "Egypt",
                Status = TripStatus.Visited,
                StartDate = DateTime.UtcNow.AddMonths(-2).AddDays(-6),
                EndDate = DateTime.UtcNow.AddMonths(-2),
                VisitedDate = DateTime.UtcNow.AddMonths(-2),
                Notes = "Visited the pyramids",
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.TripBucketItems.AddRangeAsync(tripItems);

        await context.SaveChangesAsync();
    }
}