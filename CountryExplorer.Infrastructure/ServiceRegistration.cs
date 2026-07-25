using CountryExplorer.Application.Interfaces.Repositories;
using CountryExplorer.Application.Interfaces.Services;
using CountryExplorer.Application.Interfaces.External;
using CountryExplorer.Infrastructure.ExternalServices;
using CountryExplorer.Infrastructure.Repositories;
using CountryExplorer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CountryExplorer.Infrastructure.Services;
using CountryExplorer.Application.Services;

namespace CountryExplorer.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.EnableRetryOnFailure()));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDestinationRepository, DestinationRepository>();
        services.AddScoped<ITripRepository, TripRepository>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IGoogleCalendarService, GoogleCalendarService>();


        services.AddHttpClient<ICountryApiService, CountryApiService>(client =>
        {
            client.BaseAddress = new Uri("https://api.restcountries.com/countries/v5/");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {configuration["RestCountries:ApiKey"]}");

        });

        services.AddHttpClient<ITouristAttractionService, TouristAttractionService>(client =>
        {
            client.BaseAddress = new Uri("https://api.opentripmap.com/0.1/en/places/");
        });

        services.AddHttpClient<IExchangeRateService, ExchangeRateService>(client =>
        {
            client.BaseAddress = new Uri("https://v6.exchangerate-api.com/v6/");
        });
                return services;
            }
}
