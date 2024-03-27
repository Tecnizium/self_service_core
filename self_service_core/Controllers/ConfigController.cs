using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace self_service_core.Controllers;
[ApiController]
[Route("[controller]/[action]")]
public class ConfigController : ControllerBase
{
    private readonly ILogger<ConfigController> _logger;
    private readonly IConfiguration _configuration;
    
    public ConfigController(ILogger<ConfigController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [Authorize(Policy = "Basic")]
    [HttpGet]
    public IActionResult GetConfig()
    {
        var config = _configuration.GetSection("Company");
        //"Company": {
        // "Name": "Eco",
        // "Cnpj": "12345678901234",
        // "Fee": {
        //   "Value": 0.03,
        //   "Type": 2 //Fee Type: 0 - Company, 1 - Customer, 2 - Both
        // },
        return Ok(new
        {
            Name = config.GetSection("Name").Value,
            Cnpj = config.GetSection("Cnpj").Value,
            Fee = new
            {
                Value = config.GetSection("Fee").GetSection("Value").Value,
                Type = config.GetSection("Fee").GetSection("Type").Value
            }
        });
    }

}