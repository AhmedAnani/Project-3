using CountryExplorer.Domain.Entities;
using CountryExplorer.Domain.Enums;
using CountryExplorer.Infrastructure.Data;

namespace CountryExplorer.Infrastructure.Data.Seeding;

public static class DbSeed
{
<<<<<<< HEAD:CountryExplorer.Infrastructure/seeding/DbSeed.cs
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
=======
    public static async Task SeedAsync(AppDbContext context)
    {
        if (context.Users.Any())
            return;

        var adminUser = new User
        {
            Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
            FullName = "Admin",
            Email = "admin@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        };

        var normalUser = new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            FullName = "Esraa",
            Email = "esraa@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Esraa@123"),
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };
>>>>>>> origin/main:CountryExplorer.Infrastructure/Data/Seeding/DbSeed.cs

        //    await context.Users.AddRangeAsync(adminUser, normalUser);

<<<<<<< HEAD:CountryExplorer.Infrastructure/seeding/DbSeed.cs
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
=======
        var tripItems = new List<TripBucketItem>
        {
            new()
            {
                UserId = normalUser.Id,
                Title = "Japan Spring Trip",
                CountryCode = "JP",
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
                CountryCode = "EG",
                CountryName = "Egypt",
                Status = TripStatus.Visited,
                StartDate = DateTime.UtcNow.AddMonths(-2).AddDays(-6),
                EndDate = DateTime.UtcNow.AddMonths(-2),
                VisitedDate = DateTime.UtcNow.AddMonths(-2),
                Notes = "Visited the pyramids",
                CreatedAt = DateTime.UtcNow
            }
        };
>>>>>>> origin/main:CountryExplorer.Infrastructure/Data/Seeding/DbSeed.cs

        //await context.TripBucketItems.AddRangeAsync(tripItems);

        //await context.SaveChangesAsync();
    }
