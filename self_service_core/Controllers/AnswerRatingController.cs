using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using self_service_core.Models;
using self_service_core.Services;

namespace self_service_core.Controllers;

public class AnswerRatingController : ControllerBase
{
    private readonly ILogger<AnswerRatingController> _logger;
    private readonly IMongoDbService _mongoDbService;

    public AnswerRatingController(ILogger<AnswerRatingController> logger, IMongoDbService mongoDbService)
    {
        _logger = logger;
        _mongoDbService = mongoDbService;
    }

    [Authorize(Policy = "Basic")]
    [HttpPost]
    public async Task<IActionResult> CreateAnswerRating([FromBody] IEnumerable<AnswerRatingModel> answersRating)
    {
        foreach (AnswerRatingModel answerRating in answersRating)
        {
            await _mongoDbService.CreateAnswerRating(answerRating);
        }

        return Ok();
        
    }
    
    
}