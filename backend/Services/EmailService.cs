using System.Net;
using System.Text.RegularExpressions;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Adopta.Api.Services;

public interface IEmailService
{
    /// <param name="replyTo">Opcional: a quién se responde al darle "Responder" al correo.</param>
    Task EnviarAsync(string para, string asunto, string html, string? replyTo = null);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config) => _config = config;

    public async Task EnviarAsync(string para, string asunto, string html, string? replyTo = null)
    {
        var s = _config.GetSection("Email");
        var fromAddress = s["From"] ?? throw new InvalidOperationException("Email:From must be configured.");
        var host = s["Host"] ?? throw new InvalidOperationException("Email:Host must be configured.");
        var user = s["User"] ?? throw new InvalidOperationException("Email:User must be configured.");
        var password = s["Password"] ?? throw new InvalidOperationException("Email:Password must be configured.");

        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(s["FromName"], fromAddress));
        msg.To.Add(MailboxAddress.Parse(para));
        if (!string.IsNullOrWhiteSpace(replyTo)) msg.ReplyTo.Add(MailboxAddress.Parse(replyTo));
        msg.Subject = asunto;
        // HTML + versión en texto plano (mejora la entrega y la lectura en clientes sin HTML)
        msg.Body = new BodyBuilder { HtmlBody = html, TextBody = AtextoPlano(html) }.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(host, int.Parse(s["Port"] ?? "587"), SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(user, password);
        await smtp.SendAsync(msg);
        await smtp.DisconnectAsync(true);
    }

    private static string AtextoPlano(string html)
    {
        var t = Regex.Replace(html, @"<(head|style|script)[\s\S]*?</\1>", "", RegexOptions.IgnoreCase);
        t = Regex.Replace(t, @"<br\s*/?>|</p>|</tr>|</h\d>|</div>", "\n", RegexOptions.IgnoreCase);
        t = Regex.Replace(t, @"<[^>]+>", "");
        t = WebUtility.HtmlDecode(t);
        t = Regex.Replace(t, @"[ \t]+", " ");
        t = Regex.Replace(t, @"\n\s*\n\s*\n+", "\n\n");
        return t.Trim();
    }
}