using self_service_core.Models;

namespace self_service_core.Services;

public interface ICosmosDbService
{
    //Create
    Task CreateOrder(OrderModel order);
    Task AddItemToOrder(string orderId, OrderItemModel orderItem);
    Task AddItemsToOrder(string orderId, List<OrderItemModel> items);
    
    //Read
    Task<OrderModel> GetOrder(string orderId);
    Task<List<OrderModel>> GetOrders();
    Task<List<OrderModel>> GetOrdersByStatus(OrderStatus status);
    Task<List<OrderModel>> GetOrdersByCompanyCnpjAndLastDateTime(string companyCnpj, DateTime dateTime);
     
    //Update
    Task UpdateOrder(OrderModel order);
    Task UpdateOrderStatus(string orderId, OrderStatus status);
    Task UpdateItemStatus(string orderId, string itemId, OrderItemStatus status);
    
    //Delete
    Task DeleteOrder(string orderId);
    Task DeleteItem(string orderId, string itemId);
}