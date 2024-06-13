
using self_service_core.DTOs;

namespace self_service_core.Models;

public class OrderItemModel : ItemModel
{
    
    public string ItemId { get; set; } =  Guid.NewGuid().ToString();
    public string? OrderId { get; set; }
    
    public int? CardNumber { get; set; }
    public int Quantity { get; set; }
    public double? Total { get; set; }
    
    public string Observation { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public OrderItemStatus Status { get; set; }

    public OrderItemModel()
    {
        
    }
    
    public OrderItemModel(AddItemToOrderDto addItemToOrderDto)
    {
        this.Cod = addItemToOrderDto.Cod;
        this.Quantity = addItemToOrderDto.Quantity;
        this.Observation = addItemToOrderDto.Observation;
        this.Additionals = addItemToOrderDto.Additionals.Select(additional => new AdditionalItemModel(additional)).ToList();
        this.Status = OrderItemStatus.Created;
        this.CreatedAt = DateTime.Now;
    }

    public void CalculateTotal()
    {
        if (IsPromotion == null || IsPromotion == false)
        {
            Total = Price * Quantity + Additionals.Sum(additional => additional.Price);
        }
        else
        {
            Total = PromotionPrice * Quantity + Additionals.Sum(additional => additional.Price);
        }
    }

    public void ItemDetails(ItemModel item, OrderModel order)
    {
        this.Name = item.Name;
        this.Price = item.Price;
        this.PromotionPrice = item.PromotionPrice;
        this.IsPromotion = item.IsPromotion;
        this.Image = item.Image;
        this.CardNumber = order.CardNumber;
        CalculateTotal();
    }

}

public enum OrderItemStatus
{
    Created,
    Processing,
    Done,
    Canceled
}