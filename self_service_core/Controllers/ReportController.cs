using Microsoft.AspNetCore.Mvc;
using self_service_core.Services;

namespace self_service_core.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class ReportController : ControllerBase
{
    private readonly ILogger<ReportController> _logger;
    private readonly IMongoDbService _mongoDbService;
    
    public ReportController(ILogger<ReportController> logger, IMongoDbService mongoDbService)
    {
        _logger = logger;
        _mongoDbService = mongoDbService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetReportByDay(string date)
    {
        var report = await _mongoDbService.GetReportByDay(DateTime.Parse(date));
        return Ok(report);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetReportByMonth(string date)
    {
        var report = await _mongoDbService.GetReportByMonth(DateTime.Parse(date));
        
        return Ok(report);
    }
    
}