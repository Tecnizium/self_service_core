using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using self_service_core.DTOs;
using self_service_core.Models;
using self_service_core.Services;

namespace self_service_core.Controllers;
[ApiController]
[Route("[controller]/[action]")]
public class ItemController : Controller
{
    private readonly ILogger<ItemController> _logger;
    private readonly IMongoDbService _mongoDbService;
    private readonly string _imagePath = "http://192.168.0.100:5000";
    
    public ItemController(ILogger<ItemController> logger, IMongoDbService mongoDbService)
    {
        _logger = logger;
        _mongoDbService = mongoDbService;
    }
    
    private string SaveImage(IFormFile image)
    {
        var imageFileName = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() + image.FileName;
        //Save image
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", imageFileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            image.CopyToAsync(stream);
        }
        return imageFileName;
    }
    
    //Create From Form
    [Authorize(Policy = "Bearer")]
    [HttpPost]
    public async Task<IActionResult> CreateItem([FromForm]CreateItemDto itemDto)
    {
        var item = new ItemModel(itemDto);
        
        if (itemDto.Image != null)
        {
            var imageFileName = SaveImage(itemDto.Image);
            item.Image = imageFileName;
        }
        
        await _mongoDbService.CreateItem(item);
        return Ok();
    }
    
    //Read
    [Authorize(Policy = "Bearer")]
    [HttpGet]
    public async Task<IActionResult> GetItem(string cod)
    {
        var item = await _mongoDbService.GetItem(cod);

        if (item != null)
        {
            var filePath = Path.Combine(_imagePath, "images", item.Image);
            item.Image = filePath;
        }
        return Ok(item);
    }
    
    [Authorize(Policy = "Basic")]
    [HttpGet]
    public async Task<IActionResult> GetItems()
    {
        var items = await _mongoDbService.GetItems();
        
        //Get image
        foreach (var item in items)
        {
            var filePath = Path.Combine(_imagePath, "images", item.Image);
            
            //add url
            //item.Image = "https://localhost:5000/" + filePath;
            
            item.Image = filePath;
        }
        
        return Ok(items);
    }
    
    [Authorize(Policy = "Basic")]
    [HttpGet]
    public async Task<IActionResult> GetItemsMostSold()
    {
        var items = await _mongoDbService.GetItemsMostSold();
        
        //Get image
        foreach (var item in items)
        {
            var filePath = Path.Combine(_imagePath, "images", item.Image);
            item.Image = filePath;
        }
        
        return Ok(items);
    }
    
    [Authorize(Policy = "Basic")]
    [HttpGet]
    public async Task<IActionResult> GetItemsPromotion()
    {
        var items = await _mongoDbService.GetItemsPromotion();
        
        //Get image
        foreach (var item in items)
        {
            var filePath = Path.Combine(_imagePath, "images", item.Image);
            item.Image = filePath;
        }
        
        return Ok(items);
    }
    
    [Authorize(Policy = "Basic")]
    [HttpGet]
    public async Task<IActionResult> GetItemsHighlight()
    {
        var items = await _mongoDbService.GetItemsHighlight();
        
        //Get image
        foreach (var item in items)
        {
            var filePath = Path.Combine(_imagePath, "images", item.Image);
            item.Image = filePath;
        }
        
        return Ok(items);
    }
    
    //Update
    [Authorize(Policy = "Bearer")]
    [HttpPut]
    public async Task<IActionResult> UpdateItem([FromForm]UpdateItemDto itemDto)
    {
        var item = new ItemModel(itemDto);

        if (itemDto.Image != null)
        {
            var imageFileName = SaveImage(itemDto.Image);
            item.Image = imageFileName;
        }
       
        await _mongoDbService.UpdateItem(item);
        return Ok();
    }
    
    //Delete
    [Authorize(Policy = "Bearer")]
    [HttpDelete]
    public async Task<IActionResult> DeleteItem(string cod)
    {
        var item = await _mongoDbService.GetItem(cod);
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", item.Image);
        System.IO.File.Delete(filePath);
        await _mongoDbService.DeleteItem(cod);
        return Ok();
    }
    
}