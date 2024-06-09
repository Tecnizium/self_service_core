
using self_service_core.Enums;
using self_service_core.Models;

namespace self_service_core.Helpers;

public static class CalculateFee
{
    
    public static (OrderModel order, OrderItemModel item) CalculateFeeValue(OrderModel order, OrderItemModel orderItem, ItemModel item){
        orderItem.Name = item.Name;
        orderItem.Price = item.Price;
        orderItem.PromotionPrice = item.PromotionPrice;
        orderItem.IsPromotion = item.IsPromotion;
        orderItem.CalculateTotal();
        order.Total = order.Value;
        return (order, orderItem);
    }
}