using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Adopta.Api.Services;

public interface IEmailService
{
    Task EnviarAsync(string para, string asunto, string html);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config) => _config = config;

    public async Task EnviarAsync(string para, string asunto, string html)
    {
        var s = _config.GetSection("Email");
        var fromAddress = s["From"] ?? throw new InvalidOperationException("Email:From must be configured.");
        var host = s["Host"] ?? throw new InvalidOperationException("Email:Host must be configured.");
        var user = s["User"] ?? throw new InvalidOperationException("Email:User must be configured.");
        var password = s["Password"] ?? throw new InvalidOperationException("Email:Password must be configured.");

        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(s["FromName"], fromAddress));
        msg.To.Add(MailboxAddress.Parse(para));
        msg.Subject = asunto;
        msg.Body = new BodyBuilder { HtmlBody = html }.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(host, int.Parse(s["Port"] ?? "587"), SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(user, password);
        await smtp.SendAsync(msg);
        await smtp.DisconnectAsync(true);
    }
}