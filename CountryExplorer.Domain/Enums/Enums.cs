namespace CountryExplorer.Domain.Enums;

public enum UserRole
{
    User = 0,
    Admin = 1
}

public enum TripStatus
{
    Planned = 0,
    Visited = 1
}
public enum  ExpirationTime
{        
    RefreshToken = 30,
    AccessToken = 15
}