using LocalBusinessFinder.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System;

namespace LocalBusinessFinder.Services;

public class EmailSender : IEmailSender<ApplicationUser>
{
    private readonly IConfiguration _config;

    public EmailSender(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var host = _config["Smtp:Host"];
        var portStr = _config["Smtp:Port"];
        var user = _config["Smtp:Username"];
        var pass = _config["Smtp:Password"];

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            Console.WriteLine("SMTP credentials missing. Email not sent.");
            return;
        }

        if (!int.TryParse(portStr, out int port)) port = 587;

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(user, pass),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(user, "Pindi Ki Khoj"),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };
        mailMessage.To.Add(email);

        await client.SendMailAsync(mailMessage);
    }

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        var template = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #ddd; padding: 20px; border-radius: 8px;'>
                <h2 style='color: #1e3a5f;'>Welcome to Pindi Ki Khoj!</h2>
                <p>Hi {user.FullName},</p>
                <p>Thank you for registering. Please confirm your email address to activate your account and start using our services.</p>
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{confirmationLink}' style='background: #fd7e14; color: white; text-decoration: none; padding: 12px 24px; border-radius: 25px; font-weight: bold; display: inline-block;'>Confirm Email Address</a>
                </div>
                <p>If the button doesn't work, copy and paste this link into your browser:</p>
                <p style='word-break: break-all; color: #666; font-size: 14px;'>{confirmationLink}</p>
                <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;' />
                <p style='color: #999; font-size: 12px; text-align: center;'>If you did not create this account, please ignore this email.</p>
            </div>
        ";
        return SendEmailAsync(email, "Confirm your email - Pindi Ki Khoj", template);
    }

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        return SendEmailAsync(email, "Reset your password", $"Please reset your password using the following code: {resetCode}");
    }

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        return SendEmailAsync(email, "Reset your password", $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");
    }
}
