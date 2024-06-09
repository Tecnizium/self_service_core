using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using self_service_core.DTOs;
using self_service_core.Models;
using self_service_core.Services;

namespace self_service_core.Controllers;
[ApiController]
[Authorize]
[Route("[controller]/[action]")]
public class WaiterController : ControllerBase
{
    private readonly ILogger<WaiterController> _logger;
    private readonly IMongoDbService _mongoDbService;

    public WaiterController(ILogger<WaiterController> logger, IMongoDbService mongoDbService)
    {
        _logger = logger;
        _mongoDbService = mongoDbService;
    }

    [Authorize(Policy = "Basic")]
    [HttpGet]
    public async Task<IActionResult> GetWaiters()
    {
        var waiters = await _mongoDbService.GetWaiters();
        return Ok(waiters);
    }

    [Authorize(Policy = "Bearer")]
    [HttpPost]
    public async Task<IActionResult> CreateWaiter([FromBody]CreateWaiterDto waiterDto)
    {
        await _mongoDbService.CreateWaiter(new WaiterModel(waiterDto));
        return Ok();
    }

    [Authorize(Policy = "Bearer")]
    [HttpPut]
    public async Task<IActionResult> UpdateWaiter(string waiterId, [FromBody] UpdateWaiterDto waiterDto)
    {
        var waiter = new WaiterModel(waiterDto);
        waiter.WaiterId = waiterId;
        await _mongoDbService.UpdateWaiter(waiter);
        return Ok();
    }

    
    [Authorize(Policy = "Bearer")]
    [HttpDelete]
    public async Task<IActionResult> DeleteWaiter(string id)
    {
        await _mongoDbService.DeleteWaiter(id);
        return Ok();
    }
}