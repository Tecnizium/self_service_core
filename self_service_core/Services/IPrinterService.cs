using self_service_core.Models;

namespace self_service_core.Services;

public interface IPrinterService
{
    Task Print(OrderItemModel orderItem);
    Task Print(OrderModel order);
}