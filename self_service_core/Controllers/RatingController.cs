using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using self_service_core.Models;
using self_service_core.Services;

namespace self_service_core.Controllers;

[ApiController]
[Authorize]
[Route("[controller]/[action]")]
public class RatingController : ControllerBase
{
    private readonly ILogger<RatingController> _logger;
    private readonly IMongoDbService _mongoDbService;

    public RatingController(ILogger<RatingController> logger, IMongoDbService mongoDbService)
    {
        _logger = logger;
        _mongoDbService = mongoDbService;
    }


    [Authorize(Policy = "Basic")]
    [HttpGet]
    public async Task<IActionResult> GetRatings()
    {
        var ratings = await _mongoDbService.GetRatings();
        return Ok(ratings);
    }

    [Authorize(Policy = "Bearer")]
    [HttpPost]
    public async Task<IActionResult> CreateRating([FromBody] RatingModel rating)
    {
        await _mongoDbService.CreateRating(rating);
        return Ok();
    }

    [Authorize(Policy = "Bearer")]
    [HttpPut]
    public async Task<IActionResult> UpdateRating(string id, [FromBody] RatingModel rating)
    {
        rating.RatingId = id;
        await _mongoDbService.UpdateRating(rating);
        return Ok();
    }

    [Authorize(Policy = "Bearer")]
    [HttpDelete]
    public async Task<IActionResult> DeleteRating(string id)
    {
        await _mongoDbService.DeleteRating(id);
        return Ok();
    }
}