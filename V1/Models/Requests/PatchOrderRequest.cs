using MiniApp1Api.Data.Enums;

namespace MiniApp1Api.V1.Models.Requests;

public class PatchOrderRequest
{
    public OrderStatus Status { get; set; }

    public Guid UserId { get; set; }
}