using CountryExplorer.Domain.Entities;
using CountryExplorer.Domain.Enums;
using CountryExplorer.Infrastructure.Data;

namespace CountryExplorer.Infrastructure.Seeding;

public static class DbSeed
{
    //public static async Task SeedAsync(AppDbContext context)
    //{
        //    if (context.Users.Any())
        //        return; 

        //    var adminUser = new User
        //    {
        //        Id = Guid.NewGuid(),
        //        FullName = "Admin",
        //        Email = "admin@test.com",
        //        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
        //        Role = UserRole.Admin,
        //        CreatedAt = DateTime.UtcNow
        //    };

        //    var normalUser = new User
        //    {
        //        Id = Guid.NewGuid(),
        //        FullName = "Esraa",
        //        Email = "esraa@test.com",
        //        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Esraa@123"),
        //        Role = UserRole.User,
        //        CreatedAt = DateTime.UtcNow
        //    };

        //    await context.Users.AddRangeAsync(adminUser, normalUser);

        //var tripItems = new List<TripBucketItem>
        //{
        //    new()
        //    {
        //        UserId = normalUser.Id,
        //        CountryCode = "JPN",
        //        CountryName = "Japan",
        //        Status = TripStatus.Planned,
        //        TargetDate = DateTime.UtcNow.AddMonths(3),
        //        Notes = "Cherry blossom season trip",
        //        CreatedAt = DateTime.UtcNow
        //    },
        //    new()
        //    {
        //        UserId = normalUser.Id,
        //        CountryCode = "EGY",
        //        CountryName = "Egypt",
        //        Status = TripStatus.Visited,
        //        VisitedDate = DateTime.UtcNow.AddMonths(-2),
        //        Notes = "Visited the pyramids",
        //        CreatedAt = DateTime.UtcNow
        //    }
        //};

        //await context.TripBucketItems.AddRangeAsync(tripItems);

        //await context.SaveChangesAsync();
    }
