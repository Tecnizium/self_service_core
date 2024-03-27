using self_service_core.Models;

namespace self_service_core.DTOs;

public class UpdateOrderStatusDto
{
    public string OrderId { get; set; }
    public OrderStatus Status { get; set; }
}