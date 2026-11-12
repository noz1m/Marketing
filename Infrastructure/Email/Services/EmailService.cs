using System.Net;
using System.Net.Mail;
using Infrastructure.Email.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Email.Services;

public sealed class EmailService(IConfiguration configuration) : IEmailService
{
    public async Task SendPasswordResetAsync(
        string email, string token, CancellationToken cancellationToken = default)
    {
        var host = configuration["Email:SmtpHost"];
        var username = configuration["Email:Username"];
        var password = configuration["Email:Password"];
        var from = configuration["Email:From"];
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username)
            || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(from))
        {
            throw new InvalidOperationException(
                "Email SMTP is not configured. Supply Email__* environment variables.");
        }

        var port = configuration.GetValue("Email:Port", 587);
        using var smtp = new SmtpClient(host, port)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(username, password)
        };
        using var message = new MailMessage(from, email)
        {
            Subject = "Marketing Manager - password reset",
            Body = $"Your password reset token (valid for 10 minutes):\n\n{token}\n\n" +
                   "If you did not request this, ignore this email."
        };

        await smtp.SendMailAsync(message, cancellationToken);
    }
}
