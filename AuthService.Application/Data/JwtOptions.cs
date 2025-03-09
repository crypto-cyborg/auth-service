namespace AuthService.Application.Data
{
    public class JwtOptions
    {
        public required string SecretKey { get; init; }
        public int ExpiresMinutes { get; init; } = 10;
    }
}
