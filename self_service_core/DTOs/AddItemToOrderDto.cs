namespace self_service_core.DTOs;
public class AddItemToOrderDto
{
    public string Cod { get; set; }
    public int Quantity { get; set; }
    public string Observation { get; set; }
    
    public IEnumerable<CreateAdditionalItemDto> Additionals { get; set; } = new List<CreateAdditionalItemDto>();
}