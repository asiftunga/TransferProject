namespace TransferProject.BackgroundServices.Models;

public class OrderCreatedQueueItem
{
    public string UserName { get; set; }

    public string UserEmail { get; set; }

    public Guid OrderId { get; set; }

    public int Amount { get; set; }

    public string UserId { get; set; }

    public string Currency { get; set; }
}