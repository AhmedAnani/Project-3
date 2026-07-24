using CountryExplorer.Application.Interfaces.Repositories;
using CountryExplorer.Application.Interfaces.External;
using CountryExplorer.Infrastructure.ExternalServices;
using CountryExplorer.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace CountryExplorer.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<ITripRepository, TripRepository>();
        services.AddScoped<IGoogleCalendarService, GoogleCalendarService>();

        return services;
    }
}
