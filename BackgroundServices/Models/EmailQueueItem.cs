namespace MiniApp1Api.BackgroundServices.Models;

public class EmailQueueItem
{
    public string UserEmail { get; set; }

    public string UserName { get; set; }

    public string Token { get; set; }
}