using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using MimeKit;
using TransferProject.BackgroundServices.Models;
using TransferProject.Constants;
using TransferProject.Data.Enums;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace TransferProject.BackgroundServices;

public class SendOrderInfoToUsersEmailSenderBackgroundService : BackgroundService
{
    private readonly ConcurrentQueue<OrderCreatedQueueItemForUser> _orderCreatedQueueItems;
    private readonly SemaphoreSlim _signal;
    private readonly EmailSettings _emailSettings;

    public SendOrderInfoToUsersEmailSenderBackgroundService(IOptions<EmailSettings> emailSettings)
    {
        _orderCreatedQueueItems = new ConcurrentQueue<OrderCreatedQueueItemForUser>();
        _signal = new SemaphoreSlim(0);
        _emailSettings = emailSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _signal.WaitAsync(stoppingToken);

            if (_orderCreatedQueueItems.TryDequeue(out OrderCreatedQueueItemForUser orderCreatedItem))
            {
                await SendOrderInfoEmailToAdminAsync(orderCreatedItem.UserName, orderCreatedItem.Amount, orderCreatedItem.UserEmail, orderCreatedItem.Currency, orderCreatedItem.OrderType);
            }
        }
    }

    public void QueueEmail(string username, string email, int amount, string currency, OrderTypes orderType)
    {
        _orderCreatedQueueItems.Enqueue(new OrderCreatedQueueItemForUser { UserName = username, UserEmail = email, Amount = amount, Currency = currency, OrderType = orderType});
        _signal.Release();
    }

    private async Task SendOrderInfoEmailToAdminAsync(string username, int amount, string userEmail, string currency, OrderTypes orderTypes)
    {
        MimeMessage? adminMail = new();

        adminMail.From.Add(new MailboxAddress("SİPARİŞİNİZ OLUŞTURULDU!", _emailSettings.Email));
        adminMail.To.Add(new MailboxAddress($"{username}", userEmail));

        adminMail.Subject = $"BİR ADET SİPARİŞ OLUŞTURULDU.";
        adminMail.Body = new TextPart(MimeKit.Text.TextFormat.Html) {
            Text = "<b>Merhaba </b>" + username + " yeni bir siparis olusturdunuz. Tesekkur ederiz. Siparis detaylari asagida yer almaktadir :" + "<br>" + "<b>Siparis turu: </b>" + orderTypes+ "<br>" +  "<b>Currency: </b>" + currency+ "<br>" + "<b>Siparis tutari: </b>" + amount
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