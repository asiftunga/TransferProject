using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using MimeKit;
using TransferProject.BackgroundServices.Models;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace TransferProject.BackgroundServices;

public class ForgotPasswordEmailSenderBackgroundService : BackgroundService
{
    private readonly ConcurrentQueue<PasswordResetMailQueueItem> _passwordResetMailQueueItems;
    private readonly SemaphoreSlim _signal;
    private readonly EmailSettings _emailSettings;

    public ForgotPasswordEmailSenderBackgroundService(IOptions<EmailSettings> emailSettings)
    {
        _passwordResetMailQueueItems = new ConcurrentQueue<PasswordResetMailQueueItem>();
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
        }
    }

    public void QueueEmail(string username, string email, string token)
    {
        _passwordResetMailQueueItems.Enqueue(new PasswordResetMailQueueItem { UserName = username, UserEmail = email, Token = token });
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
            smtp.Connect("smtp.gmail.com", 587, true);
            smtp.Authenticate(_emailSettings.Email, _emailSettings.Password);
            smtp.Send(userEmail);
            smtp.Disconnect(true);
        }

        #region OtherWayToSendEmail
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
        #endregion
    }
}