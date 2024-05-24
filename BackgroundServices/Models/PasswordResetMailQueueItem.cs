namespace MiniApp1Api.BackgroundServices.Models;

public class PasswordResetMailQueueItem
{
    public string UserEmail { get; set; }

    public string UserName { get; set; }

    public string Token { get; set; }
}