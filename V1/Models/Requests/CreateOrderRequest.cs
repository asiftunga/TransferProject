using MiniApp1Api.Data.Enums;

namespace MiniApp1Api.V1.Models.Requests;

public class CreateOrderRequest
{
    public Guid OrderId { get; set; }

    public OrderTypes OrderType { get; set; }

    public int Amount { get; set; }

    
}