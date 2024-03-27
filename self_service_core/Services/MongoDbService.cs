using System.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using self_service_core.Enums;
using self_service_core.Helpers;
using self_service_core.Models;

namespace self_service_core.Services;

public class MongoDbService : IMongoDbService
{
    
    private readonly ILogger<MongoDbService> _logger;
    private readonly IMongoCollection<OrderModel> _orders;
    private readonly IMongoCollection<ItemModel> _items;
    private readonly IMongoCollection<CategoryModel> _categories;
    private readonly IMongoCollection<AdminModel> _admin;
    private readonly IMongoCollection<PrinterModel> _printers;
    private readonly FeeType _feeType;
    private readonly double _feeValue;
    
    public MongoDbService(ILogger<MongoDbService> logger, IMongoClient client, IConfiguration configuration)
    {
        _logger = logger;
        _orders = client.GetDatabase("ordercraft").GetCollection<OrderModel>("orders");
        _items = client.GetDatabase("ordercraft").GetCollection<ItemModel>("items");
        _categories = client.GetDatabase("ordercraft").GetCollection<CategoryModel>("categories");
        _admin = client.GetDatabase("ordercraft").GetCollection<AdminModel>("admin");
        _feeType = configuration.GetSection("Company").GetSection("Fee").GetValue("Type", FeeType.Company);
        _feeValue = configuration.GetSection("Company").GetSection("Fee").GetValue("Value", 0.0);
        _printers = client.GetDatabase("ordercraft").GetCollection<PrinterModel>("printers");
        
    }
    
    //Order
    //Create
    public async Task CreateOrder(OrderModel order)
    {
        await _orders.InsertOneAsync(order);
    }
    //Read
    public async Task<OrderModel> GetOrder(string orderId)
    {
        var order = await _orders.Find(o => o.OrderId == orderId).FirstOrDefaultAsync();
        return order;
    }
    
    public async Task<List<OrderModel>> GetOrders()
    {
        var orders = await _orders.Find(o => true).ToListAsync();
        return orders;
    }
    
    public async Task<List<OrderModel>> GetOrdersByStatus(OrderStatus status)
    {
        // createAt < DateTime.Now.AddDays(-1)
        var orders = await _orders.Find(o => o.Status == status).ToListAsync();
        //var orders = await _orders.Find(o => o.CreatedAt < DateTime.Now.AddDays(-30) && o.Status == status).ToListAsync();
        return orders;
    }
    
    public async Task<List<OrderModel>> GetOrdersByMonth(DateTime month)
    {
        var orders = await _orders.Find(o => o.CreatedAt.Month == month.Month && o.CreatedAt.Year == month.Year && o.Status != OrderStatus.Canceled).ToListAsync() ?? new List<OrderModel>();
        return orders;
    }
    
    public async Task<List<OrderModel>> GetOrdersByDay(DateTime dateTime)
    {
        var orders = await _orders.Find(o => o.CreatedAt.Day == dateTime.Day && o.CreatedAt.Month == dateTime.Month && o.CreatedAt.Year == dateTime.Year && o.Status != OrderStatus.Canceled).ToListAsync() ?? new List<OrderModel>();
        return orders;
    }
    
    public async Task<List<OrderModel>> GetOrdersByCompanyCnpjAndLastDateTime(string companyCnpj, DateTime dateTime)
    {
        var orders = await _orders.Find(o => o.CompanyCnpj == companyCnpj && o.CreatedAt > dateTime).ToListAsync();
        return orders;
    }

    public async Task<List<OrderModel>> GetOrdersByCardNumberAndLastDateTime(int cardNumber, DateTime dateTime)
    {
        var orders = await _orders.Find(o => o.CardNumber == cardNumber && o.CreatedAt > dateTime).ToListAsync();
        return orders;
    }
    
    public async Task<OrderModel?> GetLastOrderByCardNumber(int cardNumber)
    {
        var orders = await _orders.Find(o => o.CardNumber == cardNumber).ToListAsync();
        return orders.LastOrDefault();
    }
    
    //Update
    public async Task UpdateOrder(OrderModel order)
    {
        await _orders.ReplaceOneAsync(o => o.OrderId == order.OrderId, order);
    }
    
    public async Task UpdateOrderStatus(string orderId, OrderStatus status)
    {
        var order = await GetOrder(orderId);
        order.Status = status;
        await UpdateOrder(order);
    }
    
    //Delete
    public async Task DeleteOrder(string orderId)
    {
        await _orders.DeleteOneAsync(o => o.OrderId == orderId);
    }
    
    //OrderItem
    //Create
    public async Task AddOrderItemToOrder(string orderId, OrderItemModel orderItem)
    {
        var order = await GetOrder(orderId);
        var item = await GetItem(orderItem.Cod);
        (order, orderItem) = CalculateFee.CalculateFeeValue(order, orderItem, item, _feeValue, _feeType);
        order.Items.Add(orderItem);
        await UpdateOrder(order);
    }
    
    public async Task AddOrderItemsToOrder(string orderId, List<OrderItemModel> items)
    {
        var order = await GetOrder(orderId);

        foreach (var item in items)
        {
            
            var itemDb = await GetItem(item.Cod);
            (order, var orderItem) = CalculateFee.CalculateFeeValue(order, item, itemDb, _feeValue, _feeType);
            orderItem.CardNumber = order.CardNumber;
            order.Items.Add(orderItem);
        }
        await UpdateOrder(order);
    }
    
    //Read
    public async Task<List<OrderItemModel>> GetOrderItemsByStatus(OrderItemStatus status)
    {
        // createAt < DateTime.Now.AddDays(-1)
        var orders = await _orders.Find(o => o.Items.Any(i => i.Status == status)).ToListAsync();
        //var orders = await _orders.Find(o => o.CreatedAt < DateTime.Now.AddDays(-30) && o.Items.Any(i => i.Status == status)).ToListAsync();
        var items = new List<OrderItemModel>();
        foreach (var order in orders)
        {
            items.AddRange(order.Items.Where(i => i.Status == status).Select(
                e =>
                {
                    e.OrderId = order.OrderId;
                    e.CardNumber = order.CardNumber;
                    return e;
                
                }));
        }
        return items;
    }
    
    public async Task<OrderItemModel?> GetOrderItemById(string itemId)
    {
        var order = await _orders.Find(o => o.Items.Any(i => i.ItemId == itemId)).FirstOrDefaultAsync();
        
        if (order == null)
        {
            return null;
        }
        
        var item = order.Items.Find(i => i.ItemId == itemId);
        return item;
    }
    
    //Update
    public async Task UpdateOrderItemStatus(string orderId, string itemId, OrderItemStatus status)
    {
        var order = await GetOrder(orderId);
        var item = order.Items.Find(i => i.ItemId == itemId);
        if (status == OrderItemStatus.Canceled)
        {
            order.Total = order.Total - item.Total;
        }
        if (item.Status == OrderItemStatus.Canceled && status != OrderItemStatus.Canceled)
        {
            order.Total = order.Total + item.Total;
        }
        item.Status = status;
        
        await UpdateOrder(order);
    }
    
    //Delete
    public async Task DeleteOrderItem(string orderId, string itemId)
    {
        var order = await GetOrder(orderId);
        order.Items.RemoveAll(i => i.ItemId == itemId);
        await UpdateOrder(order);
    }
    
    //Item
    //Create
    public async Task CreateItem(ItemModel item)
    {
        await _items.InsertOneAsync(item);
    }
    
    //Read
    public async Task<ItemModel> GetItem(string cod)
    {
        var item = await _items.Find(i => i.Cod == cod).FirstOrDefaultAsync();
        return item;
    }
    public async Task<List<ItemModel>> GetItems()
    {
        var items = await _items.Find(i => true).ToListAsync();
        return items;
    }
    
    public async Task<List<ItemModel>> GetItemsMostSold()
    {
        //Items most request
        var items = _orders.AsQueryable()
            .SelectMany(o => o.Items)
            .GroupBy(i => i.Cod)
            .Select(g => new {Cod = g.Key, Quantity = g.Sum(i => i.Quantity)})
            .OrderByDescending(i => i.Quantity)
            .Take(10)
            .ToList();
        var itemsMostSold = new List<ItemModel>();
        foreach (var item in items)
        {
            var itemDb = await GetItem(item.Cod);
            itemsMostSold.Add(itemDb);
        }
        return itemsMostSold;
    }
    
    public async Task<List<ItemModel>> GetItemsPromotion()
    {
        var items = await _items.Find(i => i.IsPromotion == true).ToListAsync();
        return items;
    }
    
    public async Task<List<ItemModel>> GetItemsHighlight()
    {
        var items = await _items.Find(i => i.IsHighlight == true).ToListAsync();
        return items;
    }
    
    //Update
    public async Task UpdateItem(ItemModel item)
    {
        ItemModel itemDb = await GetItem(item.Cod);
        if (item.Name == null)
            item.Name = itemDb.Name;
        if (item.Price == null)
            item.Price = itemDb.Price;
        if (item.Image == null)
            item.Image = itemDb.Image;
        if (item.CategoryId == null)
            item.CategoryId = itemDb.CategoryId;
        await _items.ReplaceOneAsync(i => i.Cod == item.Cod, item);
    }
    
    //Delete
    public async Task DeleteItem(string cod)
    {
        await _items.DeleteOneAsync(i => i.Cod == cod);
    }
    
    
    //Category
    //Create
    public async Task CreateCategory(CategoryModel category)
    {
        await _categories.InsertOneAsync(category);
    }
    
    //Read
    public async Task<CategoryModel> GetCategory(string categoryCod)
    {
        var category = await _categories.Find(c => c.CategoryId == categoryCod).FirstOrDefaultAsync();
        return category;
    }
    
    public async Task<List<CategoryModel>> GetCategories()
    {
        var categories = await _categories.Find(c => true).ToListAsync();
        return categories;
    }
    
    //Update
    public async Task UpdateCategory(CategoryModel category)
    {
        await _categories.ReplaceOneAsync(c => c.CategoryId == category.CategoryId, category);
    }
    
    //Delete
    public async Task DeleteCategory(string categoryId)
    {
        await _items.DeleteManyAsync(i => i.CategoryId == categoryId);
        await _categories.DeleteOneAsync(c => c.CategoryId == categoryId);
    }
    
    //Admin
    //Create
    public async Task CreateAdmin(AdminModel admin)
    {
        admin.Password = BCrypt.Net.BCrypt.HashPassword(admin.Password);
        await _admin.InsertOneAsync(admin);
    }
    
    //Read
    public async Task<AdminModel?> GetAdmin(string username)
    {
        var admin = await _admin.Find(a => a.Username == username).FirstOrDefaultAsync();
        return admin;
    }
    
    //Report 
    public async Task<ReportModel?> GetReportByMonth(DateTime month)
    {
        var orders = await GetOrdersByMonth(month);
        if (orders.Count == 0)
        {
            return null;
        }
        var report = new ReportModel();
        report.Quantity = orders.Count;
        report.Total = orders.Sum(o => o.Total ?? 0);
        report.AverageTicket = report.Total / report.Quantity;
        report.AverageOrderByDay = (double)report.Quantity / (double)DateTime.DaysInMonth(month.Year, month.Month);
        report.MostSoldItems = new List<ReportItemModel>();
        var items = orders.SelectMany(o => o.Items).GroupBy(i => i.Cod).Select(g => new {Cod = g.Key, Quantity = g.Sum(i => i.Quantity), Total = g.Sum(i => i.Total ?? 0)}).OrderByDescending(i => i.Quantity).Take(5).ToList();
        foreach (var item in items)
        {
            var itemDb = await GetItem(item.Cod);
            report.MostSoldItems.Add(new ReportItemModel
            {
                ItemCod = item.Cod,
                ItemName = itemDb.Name,
                Quantity = item.Quantity,
                Total = item.Total
            });
        }
        return report;
    }
    
    public async Task<ReportModel?> GetReportByDay(DateTime dateTime)
    {
        var orders = await GetOrdersByDay(dateTime);
        if (orders.Count == 0)
        {
            return null;
        }
        var report = new ReportModel();
        report.Quantity = orders.Count;
        report.Total = orders.Sum(o => o.Total ?? 0);
        report.AverageTicket = report.Total / report.Quantity;
        report.AverageOrderByDay = report.Quantity;
        report.MostSoldItems = new List<ReportItemModel>();
        var items = orders.SelectMany(o => o.Items).GroupBy(i => i.Cod).Select(g => new {Cod = g.Key, Quantity = g.Sum(i => i.Quantity), Total = g.Sum(i => i.Total ?? 0)}).OrderByDescending(i => i.Quantity).Take(5).ToList();
        foreach (var item in items)
        {
            var itemDb = await GetItem(item.Cod);
            report.MostSoldItems.Add(new ReportItemModel
            {
                ItemCod = item.Cod,
                ItemName = itemDb.Name,
                Quantity = item.Quantity,
                Total = item.Total
            });
        }
        return report;
    }
    
    
    //Printers
    //Create
    public async Task CreatePrinter(PrinterModel printer)
    {
        await _printers.InsertOneAsync(printer);
    }
    
    //Read
    public async Task<List<PrinterModel>> GetPrinters(string? categoryId)
    {
        if (categoryId == null)
        {
            var printers = await _printers.Find(p => true).ToListAsync();
            return printers ?? [];
        }
        else
        {
            var printers = await _printers.Find(p => p.CategoryIds != null && p.CategoryIds.Contains(categoryId)).ToListAsync();
            return printers ?? [];
        }
    }
    



}