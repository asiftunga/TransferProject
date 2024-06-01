using MiniApp1Api.Data.Enums;

namespace MiniApp1Api.Data.Entities;

public class Order
{
    public int Id { get; set; }

    public Guid OrderId { get; set; }

    public string UserId { get; set; }

    public int Amount { get; set; }

    public OrderTypes OrderTypes { get; set; }

    public Currencys Currency { get; set; }

    public PaymentMethods Payment { get; set; }

    public PaymentAreas PaymentArea { get; set; }

    public OrderStatus OrderStatus { get; set; }

    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string UpdatedBy { get; set; }
}