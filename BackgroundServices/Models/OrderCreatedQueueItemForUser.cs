using TransferProject.Data.Enums;

namespace TransferProject.BackgroundServices.Models;

public class OrderCreatedQueueItemForUser
{
    public string UserName { get; set; }

    public string UserEmail { get; set; }

    public int Amount { get; set; }

    public string Currency { get; set; }

    public OrderTypes OrderType { get; set; }
}