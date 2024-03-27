using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using self_service_core.Models;
using self_service_core.Services;

namespace self_service_core.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class PrinterController : ControllerBase
{
    private readonly ILogger<PrinterController> _logger;
    private readonly IMongoDbService _mongoDbService;
    
    public PrinterController(ILogger<PrinterController> logger, IMongoDbService mongoDbService)
    {
        _logger = logger;
        _mongoDbService = mongoDbService;
    }
    
    
    
    
    [HttpPost]
    public async Task<IActionResult> PrintItem(string id)
    {
        OrderItemModel? orderItem = await _mongoDbService.GetOrderItemById(id);
        
        if (orderItem == null)
        {
            return BadRequest("Order item not found");
        }
        
        
        List<PrinterModel> printers = await _mongoDbService.GetPrinters(orderItem.CategoryId);
        
        if (printers.Count == 0)
        {
            return BadRequest("No printer found");
        }
        
        
        foreach (var printer in printers)
        {
            PrinterService printerService = new PrinterService(printer);
            try
            {
                await printerService.Print(orderItem);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            
        }
        return Ok();
        
    }
    
    [HttpPost]
    public async Task<IActionResult> PrintOrder(string id)
    {
        OrderModel order = await _mongoDbService.GetOrder(id);
        List<PrinterModel> printers = await _mongoDbService.GetPrinters(null);
        printers = printers.Where(printer => printer.isDefault ?? false).ToList();
        foreach (var printer in printers)
        {
            PrinterService printerService = new PrinterService(printer);
            await printerService.Print(order);
        }
        return Ok();
        
    }
    
    [HttpPost]
    public IActionResult AddPrinter([FromBody] PrinterModel printer)
    {
        _mongoDbService.CreatePrinter(printer);
        return Ok();
    }
    
}