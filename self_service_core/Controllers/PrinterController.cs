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
    private readonly IPrinterService _printerService;

    public PrinterController(ILogger<PrinterController> logger, IMongoDbService mongoDbService, IPrinterService printerService)
    {
        _logger = logger;
        _mongoDbService = mongoDbService;
        _printerService = printerService;
    }
    

    [HttpPost]
    public IActionResult AddPrinters([FromBody] IEnumerable<PrinterModel> printer)
    {
        foreach (var printerModel in printer)
        {
            _mongoDbService.CreatePrinter(printerModel);
        }
        return Ok();
    }

    [HttpDelete]
    public IActionResult DeletePrinter(string printerIP)
    {

        _mongoDbService.DeletePrinter(printerIP);

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetPrinters()
    {
        var printers = await _mongoDbService.GetPrinters(null);
        return Ok(printers);
    }

}