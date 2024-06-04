using MiniApp1Api.Data.Enums;

namespace MiniApp1Api.V1.Models.Responses;

public class GetSingleCardOrderResponse
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public string UserId { get; set; }

    public int Amount { get; set; }

    public OrderTypes OrderTypes { get; set; }

    public Currencys Currency { get; set; }

    public PaymentMethods Payment { get; set; }

    public OrderStatus OrderStatus { get; set; }

    public string? CardName { get; set; }

    public string? CardNumber { get; set; }

    public DateTime? CardDate { get; set; }

    public string? CVV { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}