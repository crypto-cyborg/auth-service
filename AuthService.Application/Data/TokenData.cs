namespace AuthService.Application.Data
{
    public record TokenData(
        string AccessToken,
        string RefreshToken,
        DateTime RefreshTokenExpired
    );
}
