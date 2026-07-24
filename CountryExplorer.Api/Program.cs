using CountryExplorer.Application.Interfaces.Services;
using CountryExplorer.Application.Services;
using CountryExplorer.Infrastructure.ExternalServices;
using CountryExplorer.Infrastructure.Data;
using CountryExplorer.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using CountryExplorer.Application;
using CountryExplorer.Infrastructure;
using CountryExplorer.Api.Middlewares;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// AutoMapper — scan both Application (Trip profiles) and Infrastructure (Country profiles) assemblies
builder.Services.AddAutoMapper(
    typeof(CountryExplorer.Application.Profiles.TripMappingProfile).Assembly,
    typeof(CountryExplorer.Infrastructure.Mapping.MappingProfile).Assembly
);

builder.Services.AddMemoryCache();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Application & Infrastructure DI (Trip service, repository, Google Calendar)
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Esraa's external API clients
builder.Services.AddHttpClient<ICountryApiService, CountryApiService>(client =>
{
    client.BaseAddress = new Uri("https://api.restcountries.com/countries/v5/");
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {builder.Configuration["RestCountries:ApiKey"]}");

});

builder.Services.AddHttpClient<ITouristAttractionService, TouristAttractionService>(client =>
{
    client.BaseAddress = new Uri("https://api.opentripmap.com/0.1/en/places/");
});

builder.Services.AddHttpClient<IExchangeRateService, ExchangeRateService>(client =>
{
    client.BaseAddress = new Uri("https://v6.exchangerate-api.com/v6/");
});

builder.Services.AddScoped<ICountryExplorerService, CountryExplorerService>();

var app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();

    try
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();

        await DbSeed.SeedAsync(dbContext);
    }
    catch (Exception ex)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();