using TransferProject.Data.Enums;

namespace TransferProject.Data.Entities;

public class SingleCardDetail
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

    public string CreatedBy { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string UpdatedBy { get; set; }
}