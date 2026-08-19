using backend.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace backend.Services;

public class SmtpEmailSender(
    IOptions<SmtpSettings> options,
    ILogger<SmtpEmailSender> logger
) : IEmailSender
{
    private readonly SmtpSettings _settings = options.Value;
    public async Task SendConfirmEmail(
        string email,
        string confirmLink,
        CancellationToken cancellationToken = default
    )
    {
        var message = new MimeMessage();

        message.From.Add(
            new MailboxAddress(
                _settings.FromName,
                _settings.FromEmail
            )
        );

        message.To.Add(
            MailboxAddress.Parse(email)
        );

        message.Subject = "Confirm your email";

        var body = $"""
            Hello!

            Please confirm your email address by opening this link:

            {confirmLink}

            If you did not register, ignore this email.
            """;

        message.Body = new TextPart("plain")
        {
            Text = body
        };

        try
        {
            using var client = new SmtpClient();

            await client.ConnectAsync(
                _settings.Host,
                _settings.Port,
                SecureSocketOptions.StartTls,
                cancellationToken
            );

            await client.AuthenticateAsync(
                _settings.Username,
                _settings.Password,
                cancellationToken
            );

            await client.SendAsync(
                message,
                cancellationToken
            );

            await client.DisconnectAsync(
                true,
                cancellationToken
            );

            logger.LogInformation(
                "Confirmation email sent to {Email}",
                email
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to send confirmation email to {Email}",
                email
            );
        }
    }    
}