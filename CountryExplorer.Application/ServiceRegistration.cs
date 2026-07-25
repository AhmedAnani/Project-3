using CountryExplorer.Application.DTOs.Trip;
using CountryExplorer.Application.Interfaces.Services;
using CountryExplorer.Application.Services;
using CountryExplorer.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CountryExplorer.Application;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IValidator<TripItemCreateDto>, TripItemCreateDtoValidator>();
        services.AddScoped<IValidator<TripItemUpdateDto>, TripItemUpdateDtoValidator>();
        services.AddScoped<ITripService, TripService>();
        services.AddScoped<ICountryExplorerService, CountryExplorerService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        return services;
    }
}
