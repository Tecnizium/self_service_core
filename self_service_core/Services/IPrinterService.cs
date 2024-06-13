using ESCPOS_NET;
using self_service_core.Models;

namespace self_service_core.Services;

public interface IPrinterService
{
    protected Task Print(OrderItemModel orderItem, NetworkPrinter printer);
    protected Task Print(OrderModel order, NetworkPrinter printer);
    
    Task<NetworkPrinter> SetPrinter(PrinterModel printer);
    public Task SendToPrinters(OrderModel order);
    public Task SendToPrinters(OrderItemModel orderItem);
}