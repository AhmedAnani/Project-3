using CountryExplorer.Application.Interfaces;
using CountryExplorer.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CountryExplorer.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);
        services.AddScoped<ITripService, TripService>();

        return services;
    }
}
