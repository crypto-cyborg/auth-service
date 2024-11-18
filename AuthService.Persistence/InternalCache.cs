namespace AuthService.Persistence;

public class InternalCache<TKey, TValue> where TKey : notnull
{
    public Dictionary<TKey, TValue> Data { get; } = [];
}
