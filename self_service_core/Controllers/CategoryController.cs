using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using self_service_core.DTOs;
using self_service_core.Models;
using self_service_core.Services;

namespace self_service_core.Controllers;
[ApiController]
[Route("[controller]/[action]")]
public class CategoryController : ControllerBase
{
    //CRUD
    private readonly ILogger<CategoryController> _logger;
    private readonly IMongoDbService _mongoDbService;
    
    public CategoryController(ILogger<CategoryController> logger, IMongoDbService mongoDbService)
    {
        _logger = logger;
        _mongoDbService = mongoDbService;
    }
    
    //Create
    [Authorize(Policy = "Bearer")]
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody]CreateCategoryDto category)
    {
        await _mongoDbService.CreateCategory(new CategoryModel(category));
        return Ok();
    }
    
    //Read
    [Authorize(Policy = "Basic")]
    [HttpGet]
    public async Task<IActionResult> GetCategory(string categoryId)
    {
        var category = await _mongoDbService.GetCategory(categoryId);

        if (category != null)
        {
            return Ok(category);
        }
        else
        {
            return NotFound();
        }
    }
    
    [Authorize(Policy = "Basic")]
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _mongoDbService.GetCategories();
        return Ok(categories);
    }
    
    //Update
    [Authorize(Policy = "Bearer")]
    [HttpPut]
    public async Task<IActionResult> UpdateCategory([FromBody]CategoryModel category)
    {
        await _mongoDbService.UpdateCategory(category);
        return Ok();
    }
    
    //Delete
    [Authorize(Policy = "Bearer")]
    [HttpDelete]
    public async Task<IActionResult> DeleteCategory(string categoryId)
    {
        await _mongoDbService.DeleteCategory(categoryId);
        return Ok();
    }
    
}