namespace AuthService.Application.Interfaces
{
    public interface ICacheService
    {
        Task<T> Get<T>(string key);
        Task<bool> Set<T>(string key, T value, DateTimeOffset expirationTime);
    }
}
