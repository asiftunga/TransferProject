using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using MimeKit;
using MiniApp1Api.BackgroundServices.Models;
using TransferProject.Constants;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace MiniApp1Api.BackgroundServices;

public class EmailSenderBackgroundService : BackgroundService
{
    private readonly ConcurrentQueue<PasswordResetMailQueueItem> _passwordResetMailQueueItems;
    private readonly ConcurrentQueue<OrderCreatedQueueItem> _orderCreatedQueueItems;
    private readonly SemaphoreSlim _signal;
    private readonly EmailSettings _emailSettings;

    public EmailSenderBackgroundService(IOptions<EmailSettings> emailSettings)
    {
        _passwordResetMailQueueItems = new ConcurrentQueue<PasswordResetMailQueueItem>();
        _orderCreatedQueueItems = new ConcurrentQueue<OrderCreatedQueueItem>();
        _signal = new SemaphoreSlim(0);
        _emailSettings = emailSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _signal.WaitAsync(stoppingToken);

            if (_passwordResetMailQueueItems.TryDequeue(out PasswordResetMailQueueItem resetQueueItem))
            {
                await SendResetPasswordEmail(resetQueueItem.UserName, resetQueueItem.UserEmail, resetQueueItem.Token);
            }

            if (_orderCreatedQueueItems.TryDequeue(out OrderCreatedQueueItem orderCreatedItem))
            {
                await SendOrderInfoEmailToAdminAsync(orderCreatedItem.UserName, orderCreatedItem.OrderId, orderCreatedItem.Amount, orderCreatedItem.UserId, orderCreatedItem.UserEmail);
            }
        }
    }

    public void QueueEmail(string username, string email, string token)
    {
        _passwordResetMailQueueItems.Enqueue(new PasswordResetMailQueueItem { UserName = username, UserEmail = email, Token = token });
        _signal.Release();
    }

    public void QueueEmail(string username, string email, Guid orderId, int amount, string userId)
    {
        _orderCreatedQueueItems.Enqueue(new OrderCreatedQueueItem { UserName = username, UserEmail = email, OrderId = orderId, Amount = amount, UserId = userId});
        _signal.Release();
    }

    private async Task SendResetPasswordEmail(string username, string emailAddress, string token)
    {
        MimeMessage? userEmail = new();

        userEmail.From.Add(new MailboxAddress("TMMEAL", _emailSettings.Email));
        userEmail.To.Add(new MailboxAddress(username, emailAddress));

        userEmail.Subject = $"Sifren Olusturuldu {username}";
        userEmail.Body = new TextPart(MimeKit.Text.TextFormat.Html) {
            Text = "<b>Token: </b>" + token
        };

        using (SmtpClient? smtp = new())
        {
            smtp.Connect("smtp.gmail.com", 587, false);
            smtp.Authenticate(_emailSettings.Email, _emailSettings.Password);
            smtp.Send(userEmail);
            smtp.Disconnect(true);
        }

        // using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
        // {
        //     smtpClient.Credentials = new NetworkCredential(_emailSettings.Email, _emailSettings.Password);
        //     smtpClient.EnableSsl = true;
        //
        //     var mailMessage = new MailMessage
        //     {
        //         From = new MailAddress(_emailSettings.Email),
        //         Subject = "Password Reset",
        //         Body = $"Your password reset token is: {token}",
        //         IsBodyHtml = true
        //     };
        //     mailMessage.To.Add(email);
        //
        //     await smtpClient.SendMailAsync(mailMessage);
        // }
    }

    private async Task SendOrderInfoEmailToAdminAsync(string username, Guid orderId, int amount, string userId, string userEmail)
    {
        MimeMessage? adminMail = new();

        adminMail.From.Add(new MailboxAddress("KULLANICI BIR SIPARIS OLUSTURDU!", _emailSettings.Email));
        adminMail.To.Add(new MailboxAddress("Tunga", AdminMailsConstants.Tunga));
        adminMail.To.Add(new MailboxAddress("Merdan", AdminMailsConstants.Merdan));

        adminMail.Subject = $"{username} Kullanicisi bir adet siparis olusturdu.";
        adminMail.Body = new TextPart(MimeKit.Text.TextFormat.Html) {
            Text = "<b>User Email: </b>" + userEmail + "<br>" + "<b>Siparis order Id: </b>" + orderId + "<br>" + "<b>Siparis tutari: </b>" + amount + "<br>" + "<b>userId: </b>" + userId
        };

        using (SmtpClient? smtp = new())
        {
            smtp.Connect("smtp.gmail.com", 587, false);
            smtp.Authenticate(_emailSettings.Email, _emailSettings.Password);
            smtp.Send(adminMail);
            smtp.Disconnect(true);
        }
    }
}