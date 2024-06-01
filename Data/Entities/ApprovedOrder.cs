namespace MiniApp1Api.Data.Entities;

public class ApprovedOrder
{
    public Guid Id { get; set; }

    public string UserId { get; set; }

    public Guid OrderId { get; set; }

    public bool IsRead { get; set; }
}