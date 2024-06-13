namespace self_service_core.DTOs;

public class UpdateItemDto
{
    public string? Cod { get; set; }
    public string? Name { get; set; }
    public double Price { get; set; }
    public IFormFile? Image { get; set; }
    
    public string? Description { get; set; }
    
    public double? PromotionPrice { get; set; }

    public bool? isAvailable { get; set; }
    
    public bool? IsPromotion { get; set; }
    
    public bool? IsHighlight { get; set; }
    
    public List<CreateAdditionalItemDto> Additionals { get; set; } = new List<CreateAdditionalItemDto>();
    
    public string? CategoryId { get; set; }
    
}