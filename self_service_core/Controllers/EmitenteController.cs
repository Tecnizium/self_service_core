using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using self_service_core.Models;
using self_service_core.Services;

namespace self_service_core.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class EmitenteController : Controller
{

    private readonly ILogger<EmitenteController> _logger;
    private readonly IMongoDbService _mongoDbService;

    public EmitenteController(ILogger<EmitenteController> logger, IMongoDbService mongoDbService)
    {
        _logger = logger;
        _mongoDbService = mongoDbService;
    }

    [HttpGet]
    [Authorize(Policy = "Bearer")]
    public async Task<IActionResult> GetEmitente()
    {
        var emitente = await _mongoDbService.GetEmitente();

        if (emitente != null)
        {
            return Ok(emitente);
        }
        else
        {
            return NotFound();
        }
    }

    [HttpPost]
    [Authorize(Policy = "Bearer")]
    public async Task<IActionResult> SaveEmitente([FromBody] EmitenteModel emitente)
    {
        var emitenteDb = await _mongoDbService.GetEmitente();
        if (emitenteDb != null)
        {
            emitente.Id = emitenteDb.Id;
            await _mongoDbService.UpdateEmitente(emitente);
        }
        else
        {
            await _mongoDbService.CreateEmitente(emitente);
        }
        return Ok();
    }

}