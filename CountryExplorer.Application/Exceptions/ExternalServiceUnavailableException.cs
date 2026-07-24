namespace CountryExplorer.Application.Exceptions;

public class ExternalServiceUnavailableException : AppException
{
    public ExternalServiceUnavailableException(string serviceName)
        : base($"'{serviceName}' is currently unavailable. Please try again later.") { }
}