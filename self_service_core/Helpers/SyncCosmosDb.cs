
using self_service_core.Services;

namespace self_service_core.Helpers;

public static class SyncCosmosDb
{
    public static async void Sync(IMongoDbService mongoDbService, ICosmosDbService cosmosDbService, IMemoryCacheService memoryCacheService, string companyCnpj)
    {
        try
        {
            if (VerifyInternetConnection.IsConnected())
            {
                var lastSyncDateTime = await memoryCacheService.Get<DateTime?>(CacheKeys.LastSyncDateTime.ToString());
        
                var ordersMongo = await mongoDbService.GetOrdersByCompanyCnpjAndLastDateTime(companyCnpj, lastSyncDateTime ?? DateTime.MinValue);
                var ordersCosmos = await cosmosDbService.GetOrdersByCompanyCnpjAndLastDateTime(companyCnpj, lastSyncDateTime ?? DateTime.MinValue);
       
        
                var ordersToCreate = ordersMongo.Where(o => !ordersCosmos.Any(oc => oc.OrderId == o.OrderId));
                var ordersToUpdate = ordersMongo.Where(o => ordersCosmos.Any(oc => oc.OrderId == o.OrderId));
                var ordersToDelete = ordersCosmos.Where(o => !ordersMongo.Any(oc => oc.OrderId == o.OrderId));
            
                foreach (var order in ordersToCreate)
                {
                    await cosmosDbService.CreateOrder(order);
                }
        
                foreach (var order in ordersToUpdate)
                {
                    await cosmosDbService.UpdateOrder(order);
                }
        
                foreach (var order in ordersToDelete)
                {
                    await cosmosDbService.DeleteOrder(order.OrderId);
                }
            
                await memoryCacheService.Set(CacheKeys.LastSyncDateTime.ToString(), DateTime.Now);
            }
        }
        catch (Exception e)
        {
            throw;
        }
        
       
       
        
    }

}