using MongoDB.Bson.Serialization.Attributes;
using self_service_core.DTOs;

namespace self_service_core.Models;

public class CategoryModel
{
    [BsonId]
    public string? CategoryId { get; set; } = Guid.NewGuid().ToString();
    public string? Name { get; set; }
    
    public CategoryModel(CreateCategoryDto category)
    {
        this.Name = category.Name;
    }
}