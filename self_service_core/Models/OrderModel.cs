using System.Dynamic;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using self_service_core.DTOs;

namespace self_service_core.Models;

public class OrderModel
{
    [JsonPropertyName("id")]
    [BsonId]
    public string OrderId { get; set; } = Guid.NewGuid().ToString();
    public string? Name { get; set; }
    public string? Cpf { get; set; }
    public int? SecurityCode { get; set; }
    public int CardNumber { get; set; }
    public string? CompanyCnpj { get; set; }
    public string? CompanyName { get; set; }
    public List<OrderItemModel> Items { get; set; }
    public double? Value { get; set; }
    public double Discount { get; set; }
    public double? Total { get; set; }
    
    public string? ServedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public OrderStatus Status { get; set; }

    public OrderModel(int cardNumber)
    {
        this.SecurityCode = new Random().Next(1000, 9999);
        this.Status = OrderStatus.Created;
        this.CardNumber = cardNumber;
        this.CreatedAt = DateTime.Now;
    }
    
    public OrderModel(string name, string cpf, int cardNumber)
    {
        this.SecurityCode = new Random().Next(1000, 9999);
        this.Status = OrderStatus.Created;
        this.Name = name;
        this.Cpf = cpf;
        this.CardNumber = cardNumber;
        this.CreatedAt = DateTime.Now;
    }
    
    public OrderModel(CreateOrderDto createOrderDto, IConfiguration configuration)
    {
        this.SecurityCode = new Random().Next(1000, 9999);
        this.Status = OrderStatus.Created;
        this.Name = createOrderDto.Name;
        this.Cpf = createOrderDto.Cpf;
        this.CardNumber = createOrderDto.CardNumber;
        this.CreatedAt = DateTime.Now;
        this.Items = new List<OrderItemModel>();
        this.CompanyCnpj = configuration.GetSection("Company").GetSection("Cnpj").Value;
        this.CompanyName = configuration.GetSection("Company").GetSection("Name").Value;
        this.Value = 0.0;
        this.Discount = 0.0;
        this.Total = 0.0;
        this.ServedBy = createOrderDto.ServedBy;
    }
    
}


public enum OrderStatus
{
    Created,
    Processing,
    Paid,
    Canceled
}

