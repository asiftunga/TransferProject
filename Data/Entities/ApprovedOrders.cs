namespace MiniApp1Api.Data.Entities;

public class ApprovedOrders
{
    public Guid Id { get; set; }

    public string UserId { get; set; }

    public string OrderId { get; set; }

    public bool IsRead { get; set; }
}