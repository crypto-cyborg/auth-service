namespace AuthService.Application.Services.Interfaces;

public interface IBinanceService
{
    Task<bool> ValidateKeys(string apiKey, string secretKey);
}