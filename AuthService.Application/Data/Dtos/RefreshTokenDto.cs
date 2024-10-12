namespace AuthService.Application.Data.Dtos;

public record class RefreshTokenDto
{
    public required string Token { get; init; }
    public required DateTime ExpiryDate { get; init; }
}
