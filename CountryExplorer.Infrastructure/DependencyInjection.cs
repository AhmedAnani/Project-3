using CountryExplorer.Application.Interfaces;
using CountryExplorer.Infrastructure.Repositories;
using CountryExplorer.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CountryExplorer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<ITripRepository, TripRepository>();
        services.AddScoped<IGoogleCalendarService, GoogleCalendarService>();

        return services;
    }
}
