using System;
using AuthService.Application.Interfaces;
using AuthService.Persistence;

namespace AuthService.Application.Services;

public class InternalCacheService
{
    private readonly InternalCache<string, Guid> _cache;

    public InternalCacheService(InternalCache<string, Guid> cache)
    {
        _cache = cache;
    }

    public Task<Guid> Get(string key)
    {
        if (_cache.Data.TryGetValue(key, out var value))
        {
            return Task.FromResult(value);
        }

        throw new KeyNotFoundException("Key not found");
    }

    public Task<bool> Set(string key, Guid value)
    {
        return Task.FromResult(_cache.Data.TryAdd(key, value));
    }

    public Task<bool> Remove(string key)
    {
        return Task.FromResult(_cache.Data.Remove(key));
    }
}
