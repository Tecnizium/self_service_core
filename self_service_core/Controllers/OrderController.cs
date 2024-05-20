using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using self_service_core.DTOs;
using self_service_core.Models;
using self_service_core.Services;

namespace self_service_core.Controllers;

[ApiController]
[Authorize]
[Route("[controller]/[action]")]
public class OrderController : ControllerBase
{
    private readonly ILogger<OrderController> _logger;
    private readonly IMongoDbService _mongoDbService;
    private readonly IConfiguration _configuration;

    public OrderController(ILogger<OrderController> logger, IMongoDbService mongoDbService, IConfiguration configuration)
    {
        _logger = logger;
        _mongoDbService = mongoDbService;
        _configuration = configuration;
        CloseOrders();
    }
    
    //Close all order created > 1 day
    private async Task<Task> CloseOrders()
    {

         List<OrderModel?> orders = await _mongoDbService.GetOrdersByStatusWithout24Hours(OrderStatus.Created);
        orders.AddRange(await _mongoDbService.GetOrdersByStatusWithout24Hours(OrderStatus.Processing));
        foreach (var order in orders)
        {
            if (order != null && order.CreatedAt.AddDays(1) < DateTime.UtcNow)
            {
                var orderItems =  order.Items.Where(e => e.Status is OrderItemStatus.Processing or OrderItemStatus.Created).ToList();
                foreach (var orderItem in orderItems)
                {
                    await _mongoDbService.UpdateOrderItemStatus(order.OrderId, orderItem.ItemId, OrderItemStatus.Canceled);
                }
                await _mongoDbService.UpdateOrderStatus(order.OrderId, OrderStatus.Canceled);
                
            }
        }

        return Task.CompletedTask;
    }

    //Create
    [Authorize(Policy = "Basic")]
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto createOrder)
    {
        var order = new OrderModel(createOrder, _configuration);

        //Verify if card number is available
        var lastOrder = await _mongoDbService.GetLastOrderByCardNumber(order.CardNumber!);
        if (lastOrder != null && lastOrder.Status != OrderStatus.Paid && lastOrder.Status != OrderStatus.Canceled)
        {
            return BadRequest("Card number is not available");
        }

        await _mongoDbService.CreateOrder(order);

        return Ok(order);
    }

    [Authorize(Policy = "Basic")]
    [HttpGet]
    public async Task<IActionResult> CardNumberAvailable(int cardNumber)
    {
        var order = await _mongoDbService.GetLastOrderByCardNumber(cardNumber);
        if (order == null || order.Status == OrderStatus.Paid || order.Status == OrderStatus.Canceled)
        {
            return Ok(true);
        }

        return Ok(false);
    }

    //Read
    [Authorize(Policy = "Basic")]
    [HttpGet]
    public async Task<IActionResult> GetOrder(string orderId)
    {
        var order = await _mongoDbService.GetOrder(orderId);

        return Ok(order);
    }

    [Authorize(Policy = "Bearer")]
    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await _mongoDbService.GetOrders();

        return Ok(orders);
    }

    [Authorize(Policy = "Basic")]
    [HttpPost]
    public async Task<IActionResult> GetLastOrderByCardNumber([FromHeader] int securityCode, int cardNumber)
    {
        var order = await _mongoDbService.GetLastOrderByCardNumber(cardNumber);

        if (order != null && order.SecurityCode != securityCode)
        {
            return Unauthorized();
        }

        if (order is { Status: OrderStatus.Processing })
        {
            return BadRequest("Order is processing");
        }

        return Ok(order);
    }

    [Authorize(Policy = "Bearer")]
    [HttpGet]
    public async Task<IActionResult> GetOrdersByStatus(OrderStatus status)
    {
        
        var orders = await _mongoDbService.GetOrdersByStatusWith24Hours(status);
        
        return Ok(orders);
    }



    //Update
    [HttpPut]
    public async Task<IActionResult> UpdateOrder(OrderModel order)
    {
        await _mongoDbService.UpdateOrder(order);

        return Ok();
    }

    [Authorize(Policy = "Basic")]
    [HttpPut]
    public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusDto updateOrderStatusDto)
    {
        await _mongoDbService.UpdateOrderStatus(updateOrderStatusDto.OrderId, updateOrderStatusDto.Status);

        return Ok();
    }
    

    //Delete
    [HttpDelete]
    public async Task<IActionResult> DeleteOrder(string orderId)
    {
        await _mongoDbService.DeleteOrder(orderId);

        return Ok();
    }
    

}