namespace CountryExplorer.Application.Exceptions;

public class AttractionNotFoundException : AppException
{
    public AttractionNotFoundException(string xid)
        : base($"Attraction with id '{xid}' was not found.") { }
}