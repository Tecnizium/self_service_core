namespace self_service_core.Services;
using Microsoft.Extensions.Caching.Memory;


public class MemoryCacheService : IMemoryCacheService
{
    private readonly IMemoryCache _memoryCache;

    public MemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }
    
    //Set
    public async Task Set<T>(string key, T value)
    {
        await Task.Run(() => _memoryCache.Set(key, value));
    }
    
    //Get
    public async Task<T?> Get<T>(string key)
    {
        return await Task.Run(() => _memoryCache.Get<T>(key));
    }
}

public enum CacheKeys
{
    LastSyncDateTime,
}