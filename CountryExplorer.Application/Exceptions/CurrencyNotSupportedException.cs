namespace CountryExplorer.Application.Exceptions;

public class CurrencyNotSupportedException : AppException
{
    public CurrencyNotSupportedException(string currencyCode)
        : base($"Currency '{currencyCode}' is not supported.") { }
}