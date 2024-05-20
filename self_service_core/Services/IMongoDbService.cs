using self_service_core.Models;

namespace self_service_core.Services;

public interface IMongoDbService
{
    
    //Order
    //Create
    Task CreateOrder(OrderModel order);
    //Read
    Task<OrderModel> GetOrder(string orderId);
    Task<List<OrderModel>> GetOrders();
    Task<List<OrderModel>> GetOrdersByStatus(OrderStatus status);
    Task<List<OrderModel?>> GetOrdersByStatusWith24Hours(OrderStatus status);
    Task<List<OrderModel?>> GetOrdersByStatusWithout24Hours(OrderStatus status);
    Task<List<OrderModel>> GetOrdersByMonth(DateTime month);
    Task<List<OrderModel>> GetOrdersByDay(DateTime dateTime);
    Task<List<OrderModel>> GetOrdersByCompanyCnpjAndLastDateTime(string companyCnpj, DateTime dateTime);
    Task<List<OrderModel>> GetOrdersByCardNumberAndLastDateTime(int cardNumber, DateTime dateTime);
    Task<OrderModel?> GetLastOrderByCardNumber(int cardNumber);
    //Update
    Task UpdateOrder(OrderModel order);
    Task UpdateOrderStatus(string orderId, OrderStatus status);
    //Delete
    Task DeleteOrder(string orderId);
    
    
    //OrderItem
    //Create
    Task AddOrderItemToOrder(string orderId, OrderItemModel orderItem);
    Task AddOrderItemsToOrder(string orderId, List<OrderItemModel> items);
    //Read
    Task<List<OrderItemModel>> GetOrderItemsByStatusWith24Hours(OrderItemStatus status);
    Task<List<OrderItemModel>> GetOrderItemsByStatusWithout24Hours(OrderItemStatus status);
    Task<OrderItemModel?> GetOrderItemById(string itemId);
    //Update
    Task UpdateOrderItemStatus(string orderId, string itemId, OrderItemStatus status);
    //Delete
    Task DeleteOrderItem(string orderId, string itemId);
    
    
    //Item
    //Create
    Task CreateItem(ItemModel item);
    //Read
    Task<ItemModel> GetItem(string cod);
    Task<List<ItemModel>> GetItems();
    Task<List<ItemModel>> GetItemsMostSold();
    Task<List<ItemModel>> GetItemsPromotion();
    Task<List<ItemModel>> GetItemsHighlight();
    //Update
    Task UpdateItem(ItemModel item);
    //Delete
    Task DeleteItem(string cod);
    
    //Category
    //Create
    Task CreateCategory(CategoryModel category);
    //Read
    Task<CategoryModel> GetCategory(string cod);
    Task<List<CategoryModel>> GetCategories();
    //Update
    Task UpdateCategory(CategoryModel category);
    //Delete
    Task DeleteCategory(string cod);
    
    //Admin
    //Create
    Task CreateAdmin(AdminModel admin);
    //Read
    Task<AdminModel?> GetAdmin(string username);
    
    
    //Report
    //Most Sold, Quantity, Gross Profit
    Task<ReportModel?> GetReportByMonth(DateTime month);
    Task<ReportModel?> GetReportByDay(DateTime day);
    
    
    //Printer
    //Create
    Task CreatePrinter(PrinterModel printer);
    //Read
    Task<List<PrinterModel>> GetPrinters(string? categoryId);
    

}