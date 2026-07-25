namespace CountryExplorer.Application.Dtos.Auth;

public class AuthResponseDto
{
    public UserProfileDto User { get; set; } = null!;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }

    public string? GoogleAccessToken { get; set; }
}
