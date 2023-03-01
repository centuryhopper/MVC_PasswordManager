using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

public class EmailSender : IEmailSender
{
    private readonly string stmpServer;
    private readonly string port;
    private readonly string username;
    private readonly string password;
    public EmailSender(IConfiguration configuration)
    {
        stmpServer = configuration.GetSection("EmailSettings:SMTPServer").Value!;
        port = configuration.GetSection("EmailSettings:Port").Value!;
        username = configuration.GetSection("EmailSettings:Username").Value!;
        password = configuration.GetSection("EmailSettings:Password").Value!;
    }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var client = new SmtpClient(stmpServer, int.Parse(port))
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true
        };

        var mailMessage = new MailMessage(username, email, subject, message)
        {
            IsBodyHtml = true
        };

        await client.SendMailAsync(mailMessage);
    }
}
