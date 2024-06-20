using TransferProject.Data.Enums;

namespace TransferProject.Data.Entities;

public class TemporaryOrder
{
    public Guid UserId { get; set; }

    public Guid OrderId { get; set; }

    public OrderTypes OrderType { get; set; }
}