using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using self_service_core.DTOs;
using self_service_core.Models;
using self_service_core.Services;

namespace self_service_core.Controllers;
[ApiController]
[Route("[controller]/[action]")]
public class OrderItemController : ControllerBase
{
    private readonly ILogger<OrderItemController> _logger;
    private readonly IMongoDbService _mongoDbService;
    private readonly IPrinterService _printerService;
    
    public OrderItemController(ILogger<OrderItemController> logger, IMongoDbService mongoDbService, IPrinterService printerService)
    {
        _logger = logger;
        _mongoDbService = mongoDbService;
        _printerService = (PrinterService) printerService;
    }
    
    //Create
    [Authorize(Policy = "Basic")]
    [HttpPost]
    public async Task<IActionResult> AddOrderItemsToOrder(string orderId, List<AddItemToOrderDto> items)
    {
        var listItems = new List<OrderItemModel>();
        foreach (var orderItem in items)
        {
            ItemModel itemDb = await _mongoDbService.GetItem(orderItem.Cod);
            if (itemDb == null)
            {
                return BadRequest("Item not found");
            }
            OrderItemModel item  = new OrderItemModel(orderItem);
            item.OrderId = orderId;
            item.CategoryId = itemDb.CategoryId;
            item.Image = itemDb.Image;
            listItems.Add(item);
        }
        await _mongoDbService.AddOrderItemsToOrder(orderId, listItems);

        return Ok();

    }
    
    //Read
    [Authorize(Policy = "Bearer")]
    [HttpGet]
    public async Task<IActionResult> GetOrderItemsByStatus(OrderItemStatus status)
    {
        var items = await _mongoDbService.GetOrderItemsByStatusWith24Hours(status);
        items.ForEach(item => item.Image = Path.Combine("/images", item.Image!));
        return Ok(items);
    }
    
    //Update
    [Authorize(Policy = "Basic")]
    [HttpPut]
    public async Task<IActionResult> UpdateOrderItemStatus([FromBody] UpdateOrderItemStatusDto updateOrderItemStatusDto)
    {
        await _mongoDbService.UpdateOrderItemStatus(updateOrderItemStatusDto.OrderId, updateOrderItemStatusDto.OrderItemId, updateOrderItemStatusDto.Status);
        if (updateOrderItemStatusDto.Status == OrderItemStatus.Processing)
        {
            await PrintItem(updateOrderItemStatusDto.OrderItemId);
        }
        return Ok();
    }
    
    //Delete
    [Authorize(Policy = "Bearer")]
    [HttpDelete]
    public async Task<IActionResult> DeleteOrderItem(string orderId, string itemId)
    {
        await _mongoDbService.DeleteOrderItem(orderId, itemId);
        return Ok();
    }

    private async Task PrintItem(String id)
    {
        OrderItemModel? orderItem = await _mongoDbService.GetOrderItemById(id);

        if (orderItem != null) await _printerService.SendToPrinters(orderItem);
    }
}