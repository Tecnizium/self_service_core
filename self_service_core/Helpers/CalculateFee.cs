
using self_service_core.Enums;
using self_service_core.Models;

namespace self_service_core.Helpers;

public static class CalculateFee
{
    
    public static (OrderModel order, OrderItemModel item) CalculateFeeValue(OrderModel order, OrderItemModel orderItem, ItemModel item, double? feeValue, FeeType feeType){
        orderItem.Name = item.Name;
        orderItem.Price = item.Price;
        orderItem.PromotionPrice = item.PromotionPrice;
        orderItem.IsPromotion = item.IsPromotion;
        orderItem.CalculateTotal();
        order.Fee +=  orderItem.Total * feeValue;
        order.Value += ApplyFee(orderItem.Total, feeType, order.Fee);
        order.Total = order.Value + order.Fee;
        return (order, orderItem);
    }
    
    private static double? ApplyFee(double? value, FeeType feeType, double? feeValue)
    {
        if (feeType == FeeType.Company)
        {
            return value - feeValue;
        }
        else if (feeType == FeeType.Both)
        {
            return value - feeValue/2;
        }
        else
        {
            return value;
        }
    }
}