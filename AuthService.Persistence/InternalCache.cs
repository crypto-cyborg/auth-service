namespace AuthService.Persistence;

public class InternalCache<TKey, TValue>
{
    public Dictionary<TKey, TValue> Data { get; } = [];
}
