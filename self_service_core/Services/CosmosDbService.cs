using Microsoft.Azure.Cosmos;
using self_service_core.Models;

namespace self_service_core.Services;

public class CosmosDbService : ICosmosDbService
{
    
    private readonly ILogger<CosmosDbService> _logger;
    private readonly Container _orders;
    
    public CosmosDbService(ILogger<CosmosDbService> logger, CosmosClient client)
    {
        _logger = logger;
        _orders = client.GetContainer("ordercraft", "orders");
    }
    
    //Create
    public async Task CreateOrder(OrderModel order)
    {
        await _orders.CreateItemAsync(order, new PartitionKey(order.OrderId));
    }
    
    public async Task AddItemToOrder(string orderId, OrderItemModel orderItem)
    {
        var order = await GetOrder(orderId);
        order.Items.Add(orderItem);
        await UpdateOrder(order);
    }
    
    public async Task AddItemsToOrder(string orderId, List<OrderItemModel> items)
    {
        var order = await GetOrder(orderId);
        order.Items.AddRange(items);
        await UpdateOrder(order);
    }
    
    //Read
    public async Task<OrderModel> GetOrder(string orderId)
    {
        var order = await _orders.ReadItemAsync<OrderModel>(orderId, new PartitionKey(orderId));
        return order.Resource;
    }
    
    public async Task<List<OrderModel>> GetOrders()
    {
        var orders = new List<OrderModel>();
        var query = _orders.GetItemQueryIterator<OrderModel>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            orders.AddRange(response.ToList());
        }
        return orders;
    }
    
    public async Task<List<OrderModel>> GetOrdersByStatus(OrderStatus status)
    {
        var orders = new List<OrderModel>();
        var query = _orders.GetItemQueryIterator<OrderModel>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            orders.AddRange(response.Where(order => order.Status == status).ToList());
        }
        return orders;
    }
    
    public async Task<List<OrderModel>> GetOrdersByCompanyCnpjAndLastDateTime(string companyCnpj, DateTime dateTime)
    {
        var orders = new List<OrderModel>();
        var query = _orders.GetItemQueryIterator<OrderModel>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            orders.AddRange(response.Where(order => order.CompanyCnpj == companyCnpj && order.CreatedAt > dateTime).ToList());
        }
        return orders;
    }
    
    //Update
    public async Task UpdateOrder(OrderModel order)
    {
        await _orders.UpsertItemAsync(order, new PartitionKey(order.OrderId));
    }
    
    public async Task UpdateOrderStatus(string orderId, OrderStatus status)
    {
        var order = await GetOrder(orderId);
        order.Status = status;
        await UpdateOrder(order);
    }
    
    public async Task UpdateItemStatus(string orderId, string itemId, OrderItemStatus status)
    {
        var order = await GetOrder(orderId);
        var item = order.Items.Find(item => item.ItemId == itemId);
        item.Status = status;
        await UpdateOrder(order);
    }
    
    //Delete
    public async Task DeleteOrder(string orderId)
    {
        await _orders.DeleteItemAsync<OrderModel>(orderId, new PartitionKey(orderId));
    }
    
    public async Task DeleteItem(string orderId, string itemId)
    {
        var order = await GetOrder(orderId);
        order.Items.RemoveAll(item => item.ItemId == itemId);
        await UpdateOrder(order);
    }

}