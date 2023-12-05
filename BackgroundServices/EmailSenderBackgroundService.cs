using System.Collections.Concurrent;
using MiniApp1Api.BackgroundServices.Models;

namespace MiniApp1Api.BackgroundServices;

public class EmailSenderBackgroundService : BackgroundService
{
    private readonly ConcurrentQueue<EmailQueueItem> _emailQueue;
    private readonly SemaphoreSlim _signal;

    public EmailSenderBackgroundService()
    {
        _emailQueue = new ConcurrentQueue<EmailQueueItem>();
        _signal = new SemaphoreSlim(0);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _signal.WaitAsync(stoppingToken);

            if (_emailQueue.TryDequeue(out EmailQueueItem queueItem))
            {
                // Burada e-posta gönderme işlemi yapılacak
                await SendEmailAsync(queueItem.UserEmail, queueItem.Token);
            }
        }
    }

    public void QueueEmail(string email, string token)
    {
        _emailQueue.Enqueue(new EmailQueueItem { UserEmail = email, Token = token });
        _signal.Release();
    }

    private Task SendEmailAsync(string email, string token)
    {
        // Burada gerçek e-posta gönderme işlemini gerçekleştirin
        // Örneğin: SmtpClient kullanarak Gmail üzerinden e-posta gönderimi yapabilirsiniz
        // Bu kısım sizin e-posta gönderme mantığınıza bağlı olarak değişecektir
        return Task.CompletedTask;
    }
}