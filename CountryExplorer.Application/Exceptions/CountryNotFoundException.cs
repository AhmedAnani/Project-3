namespace CountryExplorer.Application.Exceptions;

public class CountryNotFoundException : AppException
{
    public CountryNotFoundException(string identifier)
        : base($"Country '{identifier}' was not found.") { }
}