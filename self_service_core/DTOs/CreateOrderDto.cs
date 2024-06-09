
namespace self_service_core.DTOs;

public class CreateOrderDto
{
    
    public string? Name { get; set; }
    public string? Cpf { get; set; }
    public int CardNumber { get; set; }
    
    public string ServedBy { get; set; }
}