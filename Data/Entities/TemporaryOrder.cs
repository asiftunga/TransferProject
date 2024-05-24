using MiniApp1Api.Data.Enums;

namespace MiniApp1Api.Data.Entities;

public class TemporaryOrder
{
    public Guid UserId { get; set; }

    public Guid OrderId { get; set; }

    public OrderTypes OrderType { get; set; }
}