using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using self_service_core.DTOs;
using self_service_core.Models;
using self_service_core.Services;

namespace self_service_core.Controllers;
[ApiController]
[Route("[controller]/[action]")]
public class AdminController : ControllerBase
{
    
    private readonly ILogger<AdminController> _logger;
    private readonly IMongoDbService _mongoDbService;
    private readonly JwtTokenService _jwtTokenService;
    
    public AdminController(ILogger<AdminController> logger, IMongoDbService mongoDbService, JwtTokenService jwtTokenService)
    {
        _logger = logger;
        _mongoDbService = mongoDbService;
        _jwtTokenService = jwtTokenService;
    }

    [Authorize(Policy = "Basic")]
    [HttpPost]
    public async Task<IActionResult> SignIn([FromBody] AdminDto admin)
    {
        var adminDb = await _mongoDbService.GetAdmin(admin.Username);
        if (adminDb != null && BCrypt.Net.BCrypt.Verify(admin.Password, adminDb.Password))
        {
            var token = _jwtTokenService.GenerateToken(adminDb.Username, "Admin");
            return Ok(new {
                token
            });
        }
        else
        {
            return Unauthorized();
        }
    }
    
    [Authorize(Policy = "Basic")]
    [HttpPost]
    public async Task<IActionResult> SignUp([FromBody] AdminDto admin)
    {
        await _mongoDbService.CreateAdmin(new AdminModel(admin));
        return Ok();
    }
    
    //Verify Token
    [Authorize(Policy = "Bearer")]
    [HttpGet]
    public async Task<IActionResult> VerifyToken()
    {
        return Ok();
    }
}