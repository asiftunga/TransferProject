using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using MimeKit;
using MiniApp1Api.BackgroundServices.Models;
using TransferProject.Constants;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace MiniApp1Api.BackgroundServices;

public class SendOrderInfoToAdminsEmailSenderBackgroundService : BackgroundService
{
    private readonly ConcurrentQueue<OrderCreatedQueueItem> _orderCreatedQueueItems;
    private readonly SemaphoreSlim _signal;
    private readonly EmailSettings _emailSettings;

    public SendOrderInfoToAdminsEmailSenderBackgroundService(IOptions<EmailSettings> emailSettings)
    {
        _orderCreatedQueueItems = new ConcurrentQueue<OrderCreatedQueueItem>();
        _signal = new SemaphoreSlim(0);
        _emailSettings = emailSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _signal.WaitAsync(stoppingToken);

            if (_orderCreatedQueueItems.TryDequeue(out OrderCreatedQueueItem orderCreatedItem))
            {
                await SendOrderInfoEmailToAdminAsync(orderCreatedItem.UserName, orderCreatedItem.OrderId, orderCreatedItem.Amount, orderCreatedItem.UserId, orderCreatedItem.UserEmail, orderCreatedItem.Currency);
            }
        }
    }

    public void QueueEmail(string username, string email, Guid orderId, int amount, string userId, string currency)
    {
        _orderCreatedQueueItems.Enqueue(new OrderCreatedQueueItem { UserName = username, UserEmail = email, OrderId = orderId, Amount = amount, UserId = userId, Currency = currency});
        _signal.Release();
    }

    private async Task SendOrderInfoEmailToAdminAsync(string username, Guid orderId, int amount, string userId, string userEmail, string currency)
    {
        MimeMessage? adminMail = new();

        adminMail.From.Add(new MailboxAddress("KULLANICI BİR SİPARİŞ OLUŞTURDU!", _emailSettings.Email));
        adminMail.To.Add(new MailboxAddress("Tunga", AdminMailsConstants.Tunga));
        adminMail.To.Add(new MailboxAddress("Merdan", AdminMailsConstants.Merdan));

        adminMail.Subject = $"{username} KULLANICISI BİR ADET SİPARİŞ OLUŞTURDU.";
        adminMail.Body = new TextPart(MimeKit.Text.TextFormat.Html) {
            Text = "<b>User Email: </b>" + userEmail + "<br>" + "<b>Siparis order Id: </b>" + orderId + "<br>" + "<b>Currency: </b>" + currency+ "<br>" + "<b>Siparis tutari: </b>" + amount + "<br>" + "<b>userId: </b>" + userId
        };

        using (SmtpClient? smtp = new())
        {
            smtp.Connect("smtp.gmail.com", 587, true);
            smtp.Authenticate(_emailSettings.Email, _emailSettings.Password);
            smtp.Send(adminMail);
            smtp.Disconnect(true);
        }
    }
}