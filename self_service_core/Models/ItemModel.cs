using MongoDB.Bson.Serialization.Attributes;
using self_service_core.DTOs;

namespace self_service_core.Models;

public class ItemModel
{
    [BsonId]
    public string? Cod { get; set; } = Guid.NewGuid().ToString();
    public string? Name { get; set; }
    public double? Price { get; set; }
    public string? Image { get; set; }
    public string? Description { get; set; }
    public double? PromotionPrice { get; set; }
    public bool? IsPromotion { get; set; }
    public bool? IsHighlight { get; set; }
    public List<AdditionalItemModel> Additionals { get; set; } = new List<AdditionalItemModel>();
    
    public string? CategoryId { get; set; }

    protected ItemModel()
    {
        
    }
    
    public ItemModel(string? name, double? price, string? image, string? description, double? promotionPrice, bool isPromotion, bool isHighlight, List<AdditionalItemModel> additionals, string? categoryId)
    {
        this.Name = name;
        this.Price = price;
        this.Image = image;
        this.Description = description;
        this.PromotionPrice = promotionPrice;
        this.IsPromotion = isPromotion;
        this.IsHighlight = isHighlight;
        this.Additionals = additionals;
        this.CategoryId = categoryId;
    }
    
    public ItemModel(CreateItemDto item)
    {
        this.Name = item.Name;
        this.Price = item.Price;
        this.Image = item.Image?.FileName;
        this.Description = item.Description;
        this.PromotionPrice = item.PromotionPrice;
        this.IsPromotion = item.IsPromotion;
        this.IsHighlight = item.IsHighlight;
        this.Additionals = item.Additionals.Select(additional => new AdditionalItemModel(additional)).ToList();
        this.CategoryId = item.CategoryId;
    }
    
    public ItemModel(UpdateItemDto item)
    {
        this.Cod = item.Cod;
        this.Name = item.Name;
        this.Price = item.Price;
        this.Image = item.Image?.FileName;
        this.Description = item.Description;
        this.PromotionPrice = item.PromotionPrice;
        this.IsPromotion = item.IsPromotion;
        this.IsHighlight = item.IsHighlight;
        this.Additionals = item.Additionals.Select(additional => new AdditionalItemModel(additional)).ToList();
        this.CategoryId = item.CategoryId;
    }
}

public class AdditionalItemModel
{
    public string? Name { get; set; }
    public double? Price { get; set; }
    
    public AdditionalItemModel(string? name, double price)
    {
        this.Name = name;
        this.Price = price;
    }
    
    public AdditionalItemModel(CreateAdditionalItemDto additional)
    {
        this.Name = additional.Name;
        this.Price = additional.Price;
    }
}