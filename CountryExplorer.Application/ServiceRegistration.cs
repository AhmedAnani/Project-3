using CountryExplorer.Application.DTOs;
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

        return services;
    }
}
