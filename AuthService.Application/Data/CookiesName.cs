namespace AuthService.Application.Data;

public class CookiesName
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
}