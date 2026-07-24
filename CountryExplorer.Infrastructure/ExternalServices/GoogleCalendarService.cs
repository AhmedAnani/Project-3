using CountryExplorer.Application.Interfaces.External;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Logging;

namespace CountryExplorer.Infrastructure.ExternalServices;

public class GoogleCalendarService : IGoogleCalendarService
{
    private const string ApplicationName = "CountryExplorer";
    private readonly ILogger<GoogleCalendarService> _logger;

    public GoogleCalendarService(ILogger<GoogleCalendarService> logger)
    {
        _logger = logger;
    }

    public async Task<string?> ScheduleTripEventAsync(
        string googleAccessToken,
        string title,
        string? notes,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(googleAccessToken))
            {
                return null;
            }

            var service = CreateCalendarService(googleAccessToken);
            var startUtc = startDate.ToUniversalTime().Date;
            var endUtc = endDate.ToUniversalTime().Date;

            var calendarEvent = new Event
            {
                Summary = title,
                Description = notes,
                Start = new EventDateTime
                {
                    Date = startUtc.ToString("yyyy-MM-dd")
                },
                End = new EventDateTime
                {
                    Date = endUtc.AddDays(1).ToString("yyyy-MM-dd")
                }
            };

            var request = service.Events.Insert(calendarEvent, "primary");
            var createdEvent = await request.ExecuteAsync(cancellationToken);

            return createdEvent.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule Google Calendar event '{Title}'.", title);
            return null;
        }
    }

    public async Task<bool> UpdateTripEventAsync(
        string googleAccessToken,
        string googleEventId,
        string title,
        string? notes,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(googleAccessToken) || string.IsNullOrWhiteSpace(googleEventId))
            {
                return false;
            }

            var service = CreateCalendarService(googleAccessToken);
            var startUtc = startDate.ToUniversalTime().Date;
            var endUtc = endDate.ToUniversalTime().Date;

            var calendarEvent = new Event
            {
                Summary = title,
                Description = notes,
                Start = new EventDateTime
                {
                    Date = startUtc.ToString("yyyy-MM-dd")
                },
                End = new EventDateTime
                {
                    Date = endUtc.AddDays(1).ToString("yyyy-MM-dd")
                }
            };

            var request = service.Events.Update(calendarEvent, "primary", googleEventId);
            await request.ExecuteAsync(cancellationToken);
            return true;
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404") || ex.Message.Contains("410"))
        {
            _logger.LogWarning("Google Calendar event {GoogleEventId} not found (404/410). Treating as deleted.", googleEventId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Google Calendar event {GoogleEventId}.", googleEventId);
            return false;
        }
    }

    public async Task<bool> DeleteTripEventAsync(
        string googleAccessToken,
        string googleEventId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(googleAccessToken) || string.IsNullOrWhiteSpace(googleEventId))
            {
                return false;
            }

            var service = CreateCalendarService(googleAccessToken);
            var request = service.Events.Delete("primary", googleEventId);
            await request.ExecuteAsync(cancellationToken);
            return true;
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("404") || ex.Message.Contains("410"))
        {
            _logger.LogWarning("Google Calendar event {GoogleEventId} not found (404/410). Treating as deleted.", googleEventId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete Google Calendar event {GoogleEventId}.", googleEventId);
            return false;
        }
    }

    private static CalendarService CreateCalendarService(string googleAccessToken)
    {
        var credential = GoogleCredential.FromAccessToken(googleAccessToken);

        return new CalendarService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName
        });
    }
}
