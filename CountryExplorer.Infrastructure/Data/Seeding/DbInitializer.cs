using CountryExplorer.Domain.Entities;
using CountryExplorer.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CountryExplorer.Infrastructure.Data.Seeding;

/// <summary>
/// Seeds the Destinations table with 12 test cities for the recommendation engine.
/// Idempotent — skips seeding if data already exists.
/// </summary>
public static class DbInitializer
{
    public static void InitializeDestinations(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        if (db.Destinations.Any())
        {
            logger.LogInformation("Destinations already seeded — skipping.");
            return;
        }

        var destinations = new List<Destination>
        {
            // ── Standard Winners ──────────────────────────────────────────

            // 1. Lisbon — Workation champion: great internet, safe, affordable
            new()
            {
                CountryCode = "PT",
                CityName = "Lisbon",
                Description = "Digital nomad hub in Southern Europe",
                InternetQualityScore = 82,
                SafetyIndex = 78,
                CostOfLivingIndex = 55,
                Tags = VibeTag.IsUrban | VibeTag.IsWarmClimate
            },

            // 2. Kuala Lumpur — Budget workation: fast internet, very cheap
            new()
            {
                CountryCode = "MY",
                CityName = "Kuala Lumpur",
                Description = "Southeast Asian tech hub with ultra-low costs",
                InternetQualityScore = 80,
                SafetyIndex = 62,
                CostOfLivingIndex = 32,
                Tags = VibeTag.IsUrban | VibeTag.IsWarmClimate
            },

            // 3. Rome — CultureHistory standard: iconic history, moderate infra
            new()
            {
                CountryCode = "IT",
                CityName = "Rome",
                Description = "The Eternal City — millennia of history",
                InternetQualityScore = 68,
                SafetyIndex = 65,
                CostOfLivingIndex = 60,
                Tags = VibeTag.IsHistoric | VibeTag.IsUrban
            },

            // 4. Kyoto — CultureHistory premium: top safety, high cost
            new()
            {
                CountryCode = "JP",
                CityName = "Kyoto",
                Description = "Ancient temples and traditional culture",
                InternetQualityScore = 88,
                SafetyIndex = 92,
                CostOfLivingIndex = 75,
                Tags = VibeTag.IsHistoric
            },

            // 5. Queenstown — Adventure standard: mountains, cold, mid-range cost
            new()
            {
                CountryCode = "NZ",
                CityName = "Queenstown",
                Description = "Adventure capital of New Zealand",
                InternetQualityScore = 72,
                SafetyIndex = 88,
                CostOfLivingIndex = 70,
                Tags = VibeTag.HasMountains | VibeTag.IsColdClimate
            },

            // 6. Interlaken — Adventure premium: mountains, safe, expensive
            new()
            {
                CountryCode = "CH",
                CityName = "Interlaken",
                Description = "Swiss Alps gateway for extreme sports",
                InternetQualityScore = 90,
                SafetyIndex = 95,
                CostOfLivingIndex = 88,
                Tags = VibeTag.HasMountains | VibeTag.IsColdClimate
            },

            // 7. Bali — Vacation budget: coastal, warm, dirt cheap
            new()
            {
                CountryCode = "ID",
                CityName = "Bali",
                Description = "Tropical paradise with vibrant culture",
                InternetQualityScore = 58,
                SafetyIndex = 60,
                CostOfLivingIndex = 28,
                Tags = VibeTag.IsCoastal | VibeTag.IsWarmClimate
            },

            // 8. Santorini — Vacation premium: coastal, warm, pricier
            new()
            {
                CountryCode = "GR",
                CityName = "Santorini",
                Description = "Iconic white-washed cliffs over the Aegean",
                InternetQualityScore = 65,
                SafetyIndex = 75,
                CostOfLivingIndex = 68,
                Tags = VibeTag.IsCoastal | VibeTag.IsWarmClimate
            },

            // ── Edge Cases ────────────────────────────────────────────────

            // 9. Alexandria — Crossover: coastal + historic + warm, budget-friendly
            new()
            {
                CountryCode = "EG",
                CityName = "Alexandria",
                Description = "Ancient Mediterranean port city",
                InternetQualityScore = 52,
                SafetyIndex = 50,
                CostOfLivingIndex = 30,
                Tags = VibeTag.IsCoastal | VibeTag.IsHistoric | VibeTag.IsWarmClimate
            },

            // 10. Cusco — Crossover with terrible internet (must fail Workation tests)
            new()
            {
                CountryCode = "PE",
                CityName = "Cusco",
                Description = "Gateway to Machu Picchu, altitude 3,400m",
                InternetQualityScore = 15,
                SafetyIndex = 48,
                CostOfLivingIndex = 25,
                Tags = VibeTag.HasMountains | VibeTag.IsHistoric
            },

            // 11. Zurich — THE TRAP: perfect infra but max cost → must trigger IsDisqualified
            new()
            {
                CountryCode = "CH",
                CityName = "Zurich",
                Description = "Financial hub — world's most expensive city",
                InternetQualityScore = 99,
                SafetyIndex = 99,
                CostOfLivingIndex = 99,
                Tags = VibeTag.IsUrban | VibeTag.IsColdClimate
            },

            // 12. Tirana — Negative control: no tags, mediocre everything
            new()
            {
                CountryCode = "AL",
                CityName = "Tirana",
                Description = "Albanian capital — baseline control destination",
                InternetQualityScore = 50,
                SafetyIndex = 50,
                CostOfLivingIndex = 50,
                Tags = VibeTag.None
            }
        };

        db.Destinations.AddRange(destinations);
        db.SaveChanges();

        logger.LogInformation("Seeded {Count} test destinations.", destinations.Count);
    }
}
