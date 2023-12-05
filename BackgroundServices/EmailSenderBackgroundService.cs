using System.Collections.Concurrent;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
using MiniApp1Api.BackgroundServices.Models;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace MiniApp1Api.BackgroundServices;

public class EmailSenderBackgroundService : BackgroundService
{
    private readonly ConcurrentQueue<EmailQueueItem> _emailQueue;
    private readonly SemaphoreSlim _signal;
    private readonly EmailSettings _emailSettings;

    public EmailSenderBackgroundService(IOptions<EmailSettings> emailSettings)
    {
        _emailQueue = new ConcurrentQueue<EmailQueueItem>();
        _signal = new SemaphoreSlim(0);
        _emailSettings = emailSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _signal.WaitAsync(stoppingToken);

            if (_emailQueue.TryDequeue(out EmailQueueItem queueItem))
            {
                await SendEmailAsync(queueItem.UserEmail, queueItem.Token);
            }
        }
    }

    public void QueueEmail(string email, string token)
    {
        _emailQueue.Enqueue(new EmailQueueItem { UserEmail = email, Token = token });
        _signal.Release();
    }

    private async Task SendEmailAsync(string emailAddress, string token)
    {
        var email = new MimeMessage();

        email.From.Add(new MailboxAddress("TMMEAL", _emailSettings.Email));
        email.To.Add(new MailboxAddress("Selam", emailAddress));

        email.Subject = "Testing out email sending";
        email.Body = new TextPart(MimeKit.Text.TextFormat.Html) {
            Text = "<b>Selam Yarram</b>"
        };

        using (var smtp = new SmtpClient())
        {
            smtp.Connect("smtp.gmail.com", 587, false);

            // Note: only needed if the SMTP server requires authentication
            smtp.Authenticate(_emailSettings.Email, _emailSettings.Password);

            smtp.Send(email);
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
}