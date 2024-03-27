using self_service_core.Models;

namespace self_service_core.DTOs;

public class UpdateOrderItemStatusDto
{
    public string OrderId { get; set; }
    public string OrderItemId { get; set; }
    public OrderItemStatus Status { get; set; }
}