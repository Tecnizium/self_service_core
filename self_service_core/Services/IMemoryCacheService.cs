namespace self_service_core.Services;

public interface IMemoryCacheService
{
    //Set
    Task Set<T>(string key, T? value);
    
    //Get
    Task<T?> Get<T>(string key);
}