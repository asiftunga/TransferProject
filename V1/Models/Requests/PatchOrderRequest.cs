using TransferProject.Data.Enums;

namespace TransferProject.V1.Models.Requests;

public class PatchOrderRequest
{
    public OrderStatus Status { get; set; }

    public Guid UserId { get; set; }
}