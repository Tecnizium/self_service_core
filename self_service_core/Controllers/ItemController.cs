using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using self_service_core.DTOs;
using self_service_core.Models;
using self_service_core.Services;
using Newtonsoft.Json.Linq;


namespace self_service_core.Controllers;
[ApiController]
[Route("[controller]/[action]")]
public class ItemController : Controller
{
    private readonly ILogger<ItemController> _logger;
    private readonly IMongoDbService _mongoDbService;
    
    public ItemController(ILogger<ItemController> logger, IMongoDbService mongoDbService, IConfiguration config)
    {
        _logger = logger;
        _mongoDbService = mongoDbService;
        CreateImageFolder();
    }
    



    //create Image folder in wwwroot
    private Task CreateImageFolder()
    {
        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        return Task.CompletedTask;
    }
    
    private async Task<string> SaveImage(IFormFile image)
    {
        var imageFileName = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() + image.FileName;
        //Save image
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", imageFileName);
        await using var stream = new FileStream(filePath, FileMode.Create);
        await image.CopyToAsync(stream);
        return imageFileName;
    }
    
    private Task DeleteImage(string imageFileName)
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", imageFileName);
        System.IO.File.Delete(filePath);
        return Task.CompletedTask;
    }
    
    //Create From Form
    [Authorize(Policy = "Bearer")]
    [HttpPost]
    public async Task<IActionResult> CreateItem([FromForm]CreateItemDto itemDto)
    {
        var item = new ItemModel(itemDto);
        
        if (itemDto.Image != null)
        {
            var imageFileName = await SaveImage(itemDto.Image);
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
            var filePath = Path.Combine("/images", item.Image);
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
            var filePath = Path.Combine("/images", item.Image);
            
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
            var filePath = Path.Combine("/images", item.Image);
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
            var filePath = Path.Combine("/images", item.Image);
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
            var filePath = Path.Combine("/images", item.Image);
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
            //Delete old image
            var oldItem = await _mongoDbService.GetItem(itemDto.Cod!);
            await DeleteImage(oldItem.Image!);
            
            //Save new image
            var imageFileName = await SaveImage(itemDto.Image);
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
        await DeleteImage(item.Image!);
        await _mongoDbService.DeleteItem(cod);
        return Ok();
    }
    
}